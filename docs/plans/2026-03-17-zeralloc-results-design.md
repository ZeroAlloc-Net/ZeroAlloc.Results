# ZeroAlloc.Results — Design Document

**Date:** 2026-03-17

## Goal

A zero-allocation, no-boxing `Result<T, E>` library for .NET with full API parity with
CSharpFunctionalExtensions (CFE), targeting high-performance / hot-path code.

## Constraints

- All core types are `readonly struct` — no heap allocation from the Result itself
- No boxing — `IResult<T, E>` is defined but used only as a generic constraint, never as a variable or return type
- Async variants use `ValueTask<Result<T, E>>`, not `Task<>`
- Target framework: `net8.0` only (required for static abstract interface members and latest ValueTask optimizations)
- Nullable reference types enabled

## Type System

Five core types, all `readonly struct`:

| Type | Success value | Error type | Use case |
|---|---|---|---|
| `Result` | none | none | simple pass/fail |
| `Result<T>` | `T` | `string` | most common case |
| `Result<T, E>` | `T` | `E` | fully generic |
| `UnitResult<E>` | none | `E` | typed error, no value |
| `Maybe<T>` | `T` | none | optional / Option type |

### Interface

```csharp
// Constraint-only — NEVER use as a variable or return type (causes boxing)
public interface IResult<T, E>
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
    T Value { get; }
    E Error { get; }
}
```

### Internal struct layout

```csharp
public readonly struct Result<T, E> : IResult<T, E>
{
    private readonly bool _isSuccess;
    private readonly T _value;    // default(T) if failure
    private readonly E _error;    // default(E) if success
}
```

Memory footprint: `sizeof(bool) + sizeof(T) + sizeof(E)` + alignment padding. No heap allocation.

## API Surface

### Factory methods

```csharp
Result<T, E>.Success(T value)
Result<T, E>.Failure(E error)
```

### Implicit conversions

```csharp
public static implicit operator Result<T, E>(T value)
public static implicit operator Result<T, E>(E error)
```

### Core combinators

| Method | Signature | Purpose |
|---|---|---|
| `Match` | `(T→U, E→U) → U` | extract value |
| `Map` | `(T→U) → Result<U, E>` | transform value |
| `MapError` | `(E→F) → Result<T, F>` | transform error |
| `Bind` | `(T→Result<U,E>) → Result<U, E>` | chain results |
| `Tap` | `(T→void) → Result<T, E>` | side-effect on success |
| `TapError` | `(E→void) → Result<T, E>` | side-effect on failure |
| `Ensure` | `(T→bool, E) → Result<T, E>` | validation |
| `Combine` | `(ReadOnlySpan<Result<T,E>>) → Result<T,E>` | merge results (zero-alloc via span) |

### Async variants

Every combinator has an async overload returning `ValueTask`:

```csharp
ValueTask<Result<U, E>> MapAsync(Func<T, ValueTask<U>> map)
ValueTask<Result<U, E>> BindAsync(Func<T, ValueTask<Result<U, E>>> bind)
ValueTask<Result<T, E>> TapAsync(Func<T, ValueTask> tap)
ValueTask<Result<T, E>> TapErrorAsync(Func<E, ValueTask> tap)
ValueTask<U> MatchAsync(Func<T, ValueTask<U>> onSuccess, Func<E, ValueTask<U>> onFailure)
```

### LINQ support

Enables query syntax via `Select`, `SelectMany`, `Where`:

```csharp
from user in GetUser(id)
from profile in GetProfile(user)
where profile.IsActive
select profile.Name
```

## Project Structure

```
ZeroAlloc.Results/
├── src/
│   └── ZeroAlloc.Results/
│       ├── ZeroAlloc.Results.csproj
│       ├── IResult.cs
│       ├── Result.cs
│       ├── Result`1.cs            # Result<T>
│       ├── Result`2.cs            # Result<T, E>
│       ├── UnitResult.cs
│       ├── Maybe.cs
│       └── Extensions/
│           ├── ResultExtensions.cs
│           ├── ResultAsyncExtensions.cs
│           └── ResultLinqExtensions.cs
└── tests/
    └── ZeroAlloc.Results.Tests/
        ├── ZeroAlloc.Results.Tests.csproj
        ├── ResultTests.cs
        ├── MaybeTests.cs
        └── Benchmarks/
            └── AllocationBenchmarks.cs
```

## Testing Strategy

- Unit tests: `xUnit`
- Allocation verification: `BenchmarkDotNet` with `MemoryDiagnoser` — non-zero `Gen0` is a build break
- CFE compatibility: side-by-side assertions that behavior matches CSharpFunctionalExtensions for identical inputs

## NuGet

- Package ID: `ZeroAlloc.Results`
- TFM: `net8.0`
- Single package, no sub-packages
