---
id: performance
title: Performance
slug: /docs/performance
description: Zero-allocation design and benchmark results.
sidebar_position: 6
---

# Performance

ZeroAlloc.Results is built for code paths where Result overhead is measurable. This page explains why existing libraries allocate and how this library eliminates it.

## Why Most Result Libraries Allocate

Older and simpler Result libraries use class-based types:

```csharp
// Class-based Result — allocates on every call
public class Result<T, E> { ... }
```

Every `Result.Success(...)` call allocates a new object on the heap. Under load, GC minor collections become frequent.


## How ZeroAlloc.Results Eliminates Allocation

Three decisions make zero allocation possible:

**1. `readonly struct` instead of class**

```csharp
public readonly struct Result<T, E> : IResult<T, E>
{
    private readonly bool _isSuccess;
    private readonly T _value;
    private readonly E _error;
}
```

The struct lives on the stack. No heap allocation for the wrapper itself.

**2. No boxing**

`IResult<T, E>` exists only as a generic constraint — never as a variable type. This prevents the JIT from boxing the struct.

```csharp
// ✅ Zero alloc — constrained call, no boxing
void Log<TResult>(TResult r) where TResult : IResult<int, string> { ... }

// ❌ Allocates — boxes the struct
IResult<int, string> r = myResult;
```

**3. `ValueTask` for async**

All async combinators return `ValueTask<Result<T,E>>`, avoiding the `Task` allocation on synchronous completions.

## Benchmark Results

Environment: Windows 11, Intel Core i9-12900HK, .NET 10.0.7 (RyuJIT x86-64-v3), BenchmarkDotNet v0.15.4.

### Head-to-head vs OneOf / ErrorOr / FluentResults

<!-- BENCH:START -->
_Last refreshed: 2026-05-13_

| Scenario | ZeroAlloc.Results | OneOf | ErrorOr | FluentResults |
|---|---:|---:|---:|---:|
| Success construct | 0.4 ns / 0 B | 0.5 ns / 0 B | 0.0 ns / 0 B | 87 ns / **112 B** |
| Failure construct | 0.3 ns / 0 B | 0.9 ns / 0 B | 63 ns / **184 B** | 87 ns / **272 B** |
| Success consume | 0.3 ns / 0 B | 0.1 ns / 0 B | 0.2 ns / 0 B | 75 ns / **96 B** |
| Failure consume | 0.4 ns / 0 B | 0.9 ns / 0 B | 2.6 ns / 0 B | 214 ns / **240 B** |
| Hot loop (100 iter, mixed) | **183 ns / 0 B** | 202 ns / 0 B | 7,693 ns / 6,256 B | 39,450 ns / 25,968 B |

ZeroAlloc.Results is the **only library with 0 B allocation on every path** — including failure construction, which is where ErrorOr (184 B) and FluentResults (272 B) pay the most. OneOf is the closest competitor (also struct-based, also 0 B on hot paths) but is ~10% slower on the realistic mixed workload.

**The realistic-workload headline (100 iterations with 1-in-3 failures):**

- ZeroAlloc.Results: **183 ns / 0 B**
- OneOf: 202 ns / 0 B (1.1× slower)
- ErrorOr: 7,693 ns / 6,256 B (**42× slower, +∞× more alloc**)
- FluentResults: 39,450 ns / 25,968 B (**216× slower, +∞× more alloc**)

ErrorOr and FluentResults allocate per-failure because their error types (`Error` struct with description string interning + `IError` interface implementations) are non-trivial. For a CRUD app handling occasional validation errors the cost is invisible; for a hot pipeline processing tens of thousands of items where any non-trivial fraction fail, it dominates.
<!-- BENCH:END -->

### Head-to-head vs CSharpFunctionalExtensions (legacy comparison)

| Category | ZeroAlloc.Results | CSharpFunctionalExtensions | Allocated | Ratio |
|----------|------------------:|--------------:|:---------:|------:|
| `Create_Success` | 0.33 ns | 2.89 ns | **0 B** both | 8.7× faster |
| `Create_Failure` | 0.30 ns | 1.44 ns | **0 B** both | 4.8× faster |
| `Map` | 1.09 ns | 1.48 ns | **0 B** both | 1.4× faster |
| `Bind` | 5.05 ns | 4.69 ns | **0 B** both | comparable |
| `Match` | 0.37 ns | 0.68 ns | **0 B** both | 1.9× faster |
| `Chain` (Map+Bind+Match) | 2.28 ns | 2.45 ns | **0 B** both | 1.1× faster |

Run the comparison benchmark yourself:

```bash
dotnet run --project benchmarks/ZeroAlloc.Results.Benchmarks -c Release -- --filter "*ResultLibrariesBenchmark*"
dotnet run --project tests/ZeroAlloc.Results.Tests -c Release --filter "*CfeComparisonBenchmarks*"
```

## Result vs throw/catch

A separate standalone benchmarks project at [benchmarks/ZeroAlloc.Results.Benchmarks](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/tree/main/benchmarks/ZeroAlloc.Results.Benchmarks) compares the `Result<T, E>` failure path against `throw/catch` propagation.

The setup is a tight loop dividing by `i % 3` (one-in-three are zero-divides). In the throwing variant, every third iteration raises and catches a `DivideByZeroException`; in the Result variant, every third returns `Result<int, string>.Failure("div by zero")`.

```bash
dotnet run --project benchmarks/ZeroAlloc.Results.Benchmarks -c Release --filter "*"
```

What to watch:

- **Allocated column**: the Result row must read `0 B/op`. The throw/catch row allocates the exception instance plus its captured stack trace — typically 200+ bytes per raise
- **Ratio column**: even a single exception-propagation cycle runs several orders of magnitude slower than the Result path

