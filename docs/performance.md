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

Measured 2026-10-21 on an otherwise idle machine:

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9711)
12th Gen Intel Core i9-12900HK 2.50GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.402
  [Host] : .NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

> Allocations are exact. Speed is shown as same-run ratios; absolute timings are not published.

Every benchmark ran out of process in three launches, pinned to two performance cores. Ratios come from one run, with the lowest and highest of the three launches in brackets.

### Head-to-head vs OneOf / ErrorOr / FluentResults

<!-- BENCH:START -->
_Last refreshed: 2026-10-21_

**The realistic-workload headline (100 iterations with 1-in-3 failures),** each library's time relative to ZeroAlloc.Results in the same run:

| Library | Ratio | Launch range | Allocated |
|---|---:|---:|---:|
| ZeroAlloc.Results | **1.00** | | **0 B** |
| OneOf | 1.02 | 1.01–1.03 | 0 B |
| ErrorOr | 4.14 | 4.10–4.27 | 2,992 B |
| FluentResults | 41.39 | 40.10–42.43 | 25,968 B |

**Single operations,** allocated bytes per call:

| Scenario | ZeroAlloc.Results | OneOf | ErrorOr | FluentResults |
|---|---:|---:|---:|---:|
| Success construct | 0 B | 0 B | 0 B | **112 B** |
| Failure construct | 0 B | 0 B | **88 B** | **272 B** |
| Success consume | 0 B | 0 B | 0 B | **96 B** |
| Failure consume | 0 B | 0 B | 0 B | **240 B** |

Times for the single operations are not given for ZeroAlloc.Results, OneOf and ErrorOr: each takes well under a nanosecond, below what BenchmarkDotNet resolves, because the JIT reduces constructing or reading a small struct to almost nothing. The operations that allocate are measurable: FluentResults' failure construct takes about 4.5 times as long as ErrorOr's in the same run.

ZeroAlloc.Results and OneOf both allocate **0 B on every path**, including failure construction, which is where ErrorOr (88 B) and FluentResults (272 B) pay the most. Both are struct-based and run at parity on the mixed workload.

ErrorOr and FluentResults allocate per-failure because their error types (`Error` struct with description string interning + `IError` interface implementations) are non-trivial. For a CRUD app handling occasional validation errors the cost is invisible; for a hot pipeline processing tens of thousands of items where any non-trivial fraction fail, it dominates.
<!-- BENCH:END -->

### Head-to-head vs CSharpFunctionalExtensions (legacy comparison)

Measured in the same session, on .NET 9.0, the framework the test project that hosts this benchmark targets. Ratio is CSharpFunctionalExtensions' mean time relative to ZeroAlloc.Results' for the same operation.

| Category | CSharpFunctionalExtensions vs ZeroAlloc.Results | Launch range | Allocated |
|----------|------------------:|--------------:|:---------:|
| `Create_Success` | ZeroAlloc.Results below resolution ¹ | | **0 B** both |
| `Create_Failure` | ZeroAlloc.Results below resolution ¹ | | **0 B** both |
| `Map` | 1.00, parity | 0.88–1.16 | **0 B** both |
| `Bind` | 0.91, CFE ~9% faster | 0.88–0.93 | **0 B** both |
| `Match` | 0.90, CFE ~10% faster | 0.62–1.04 | **0 B** both |
| `Chain` (Map+Bind+Match) | 0.96, parity | 0.88–1.02 | **0 B** both |

¹ Creating a ZeroAlloc.Results result takes well under a nanosecond, below what BenchmarkDotNet resolves; CSharpFunctionalExtensions' is measurable, so ZeroAlloc.Results is the faster of the two by a margin this benchmark cannot size. An earlier run reported 8.7× and 4.8× from sub-nanosecond figures, and ZeroAlloc.Results ahead on `Map`, `Match` and `Chain`; with three pinned launches the two are at parity or CSharpFunctionalExtensions slightly ahead there.

Run the comparison benchmark yourself:

```bash
dotnet run --project benchmarks/ZeroAlloc.Results.Benchmarks -c Release -- --filter "*ResultLibrariesBenchmark*"
dotnet run --project tests/ZeroAlloc.Results.Tests -c Release -- --filter "*CfeComparisonBenchmarks*"
```

## Result vs throw/catch

A separate standalone benchmarks project at [benchmarks/ZeroAlloc.Results.Benchmarks](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/tree/main/benchmarks/ZeroAlloc.Results.Benchmarks) compares the `Result<T, E>` failure path against `throw/catch` propagation.

The setup is a tight loop dividing by `i % 3` (one-in-three are zero-divides). In the throwing variant, every third iteration raises and catches a `DivideByZeroException`; in the Result variant, every third returns `Result<int, string>.Failure("div by zero")`.

```bash
dotnet run --project benchmarks/ZeroAlloc.Results.Benchmarks -c Release --filter "*"
```

Measured in the same session, 100 iterations per call:

| Benchmark | Ratio | Launch range | Allocated |
|---|---:|---:|---:|
| `Result<int, string>` | 1.00 | | **0 B** |
| throw/catch | **364** | 362–366 | 6,528 B |

Throwing and catching takes 364 times as long as returning a failed `Result`, and allocates about 198 B for each of the 33 exceptions. BenchmarkDotNet reported 0 B for the throw/catch row although Gen0 collections happened during it; the 6,528 B is a per-thread measurement with `GC.GetAllocatedBytesForCurrentThread()` over 20,000 calls, the method `AllocationGate` uses.

What to watch:

- **Allocated column**: the Result row must read `0 B/op`. The throw/catch row allocates the exception instance plus its captured stack trace — typically 200+ bytes per raise
- **Ratio column**: even a single exception-propagation cycle runs several orders of magnitude slower than the Result path

