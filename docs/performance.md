---
id: performance
title: Performance
slug: /docs/performance
description: Zero-allocation design and benchmark results against CSharpFunctionalExtensions.
sidebar_position: 6
---

# Performance

ZeroAlloc.Results is built for code paths where Result overhead is measurable. This page explains why existing libraries allocate and how this library eliminates it.

## Why Result Libraries Usually Allocate

Libraries like CSharpFunctionalExtensions use class-based types:

```csharp
// CFE internally — Result<T, E> is a class
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

**ZeroAlloc.Results alone** (.NET 9.0.14, BenchmarkDotNet v0.13.12):

| Operation | Mean | Allocated |
|-----------|-----:|----------:|
| `Result<int,string>.Success(42)` | 0.26 ns | **0 B** |
| `Result<int,string>.Failure("err")` | 0.35 ns | **0 B** |
| `.Map(x => x * 2)` | 1.57 ns | **0 B** |
| `.Bind(x => Success(x.ToString()))` | 7.68 ns | **0 B** |
| `.Match(v => v, _ => -1)` | 2.27 ns | **0 B** |
| `Maybe<int>.Some(42)` | 3.47 ns | **0 B** |
| `UnitResult<string>.Success()` | 0.35 ns | **0 B** |

**Head-to-head vs CSharpFunctionalExtensions** (same machine):

> Run `dotnet run --project tests/ZeroAlloc.Results.Tests -c Release --filter "*CfeComparisonBenchmarks*"` to reproduce.

All ZeroAlloc.Results operations show `Allocated = -`. CFE operations allocate a new class instance per call.
