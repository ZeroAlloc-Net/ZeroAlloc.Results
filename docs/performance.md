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

Environment: Windows 11, Unknown processor, .NET 9.0.14 (RyuJIT AVX2), BenchmarkDotNet v0.13.12.

**ZeroAlloc.Results — zero allocation confirmed:**

| Method | Mean | Error | StdDev | Allocated |
|--------|-----:|------:|-------:|----------:|
| `Create_Success` | 0.25 ns | ±0.09 ns | ±0.28 ns | **0 B** |
| `Create_Failure` | 0.43 ns | ±0.11 ns | ±0.34 ns | **0 B** |
| `Map_Success` | 2.92 ns | ±0.40 ns | ±1.17 ns | **0 B** |
| `Bind_Chain` | 8.81 ns | ±0.60 ns | ±1.71 ns | **0 B** |
| `Match_Success` | 2.02 ns | ±0.34 ns | ±1.00 ns | **0 B** |
| `Maybe_Some` | 3.66 ns | ±0.31 ns | ±0.91 ns | **0 B** |
| `UnitResult_Success` | 0.35 ns | ±0.13 ns | ±0.38 ns | **0 B** |

**Head-to-head vs a popular Result library:**

| Category | ZeroAlloc.Results | Other library | Allocated | Ratio |
|----------|------------------:|--------------:|:---------:|------:|
| `Create_Success` | 0.33 ns | 2.89 ns | **0 B** both | 8.7× faster |
| `Create_Failure` | 0.30 ns | 1.44 ns | **0 B** both | 4.8× faster |
| `Map` | 1.09 ns | 1.48 ns | **0 B** both | 1.4× faster |
| `Bind` | 5.05 ns | 4.69 ns | **0 B** both | comparable |
| `Match` | 0.37 ns | 0.68 ns | **0 B** both | 1.9× faster |
| `Chain` (Map+Bind+Match) | 2.28 ns | 2.45 ns | **0 B** both | 1.1× faster |

Run the comparison benchmark yourself:

```bash
dotnet run --project tests/ZeroAlloc.Results.Tests -c Release --filter "*CfeComparisonBenchmarks*"
```

