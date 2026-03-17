---
id: migration-from-cfe
title: Migration from CFE
slug: /docs/migration-from-cfe
description: Drop-in migration guide from CSharpFunctionalExtensions to ZeroAlloc.Results.
sidebar_position: 7
---

# Migration from CSharpFunctionalExtensions

ZeroAlloc.Results is API-compatible with CSharpFunctionalExtensions for the common patterns. Most migrations are a find-and-replace of `using` directives.

## Namespace swap

```csharp
// Before
using CSharpFunctionalExtensions;

// After
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;
```

## Type mapping

| CFE | ZeroAlloc.Results |
|-----|------------------|
| `Result<T>` | `Result<T>` |
| `Result<T, E>` | `Result<T, E>` |
| `UnitResult<E>` | `UnitResult<E>` |
| `Maybe<T>` | `Maybe<T>` |

## Factory method mapping

| CFE | ZeroAlloc.Results |
|-----|------------------|
| `Result.Success<T, E>(value)` | `Result<T, E>.Success(value)` |
| `Result.Failure<T, E>(error)` | `Result<T, E>.Failure(error)` |
| `Result<T>.Success(value)` | `Result<T>.Success(value)` |
| `Maybe<T>.From(value)` | `Maybe<T>.Some(value)` |
| `Maybe.None` | `Maybe<T>.None` |

## Combinator mapping

All combinators have the same names and signatures:

| Method | Compatible |
|--------|-----------|
| `.Map(func)` | ✅ |
| `.MapError(func)` | ✅ |
| `.Bind(func)` | ✅ |
| `.Match(onSuccess, onFailure)` | ✅ |
| `.Tap(action)` | ✅ |
| `.TapError(action)` | ✅ |
| `.Ensure(predicate, error)` | ✅ |

## Breaking differences

**`IResult<T, E>` as a variable type causes boxing.**
CFE exposes `IResult` as a usable interface. In ZeroAlloc.Results, `IResult<T, E>` is decorated `[EditorBrowsable(Never)]` and must only be used as a generic constraint. Assign to concrete types, not to the interface.

**`Combine` takes `ReadOnlySpan` instead of `IEnumerable`.**
Pass a stack-allocated span for zero-alloc usage:

```csharp
// CFE
Result.Combine(r1, r2, r3);

// ZeroAlloc.Results (zero-alloc)
Span<Result<int, string>> results = [r1, r2, r3];
ResultExtensions.Combine<int, string>(results);
```

**Async methods return `ValueTask`, not `Task`.**
Await them as normal — `ValueTask` is awaitable. If you need a `Task`, call `.AsTask()`.
