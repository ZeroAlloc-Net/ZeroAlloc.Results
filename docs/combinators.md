---
id: combinators
title: Combinators
slug: /docs/combinators
description: Map, MapError, Bind, Match, Tap, TapError, Ensure, Combine — all zero-allocation.
sidebar_position: 3
---

# Combinators

All combinators are extension methods in `ZeroAlloc.Results.Extensions`. Every method returns a struct — no heap allocation.

```csharp
using ZeroAlloc.Results.Extensions;
```

## Map

Transforms the success value. Passes failure through unchanged.

```csharp
Result<int, string>.Success(5)
    .Map(x => x * 2);   // Success(10)

Result<int, string>.Failure("err")
    .Map(x => x * 2);   // Failure("err") — func not called
```

## MapError

Transforms the error value. Passes success through unchanged.

```csharp
Result<int, string>.Failure("err")
    .MapError(e => e.Length);   // Failure(3)
```

## Bind

Chains a result-returning function. Short-circuits on failure.

```csharp
Result<int, string>.Success(5)
    .Bind(x => x > 0
        ? Result<string, string>.Success("positive")
        : Result<string, string>.Failure("not positive"));
// Success("positive")
```

## Match

Extracts a value by applying one of two functions.

```csharp
int value = Result<int, string>.Success(42)
    .Match(v => v * 2, _ => -1);  // 84

int error = Result<int, string>.Failure("err")
    .Match(v => v * 2, e => e.Length);  // 3
```

## Tap

Executes a side-effect on success. Returns the original result unchanged.

```csharp
result
    .Tap(v => logger.Log($"Got {v}"))
    .Tap(v => metrics.Increment("success"));
```

## TapError

Executes a side-effect on failure. Returns the original result unchanged.

```csharp
result
    .TapError(e => logger.LogError($"Failed: {e}"));
```

## Ensure

Converts a successful result to failure if a predicate returns false.

```csharp
Result<int, string>.Success(-1)
    .Ensure(x => x > 0, "must be positive");  // Failure("must be positive")

Result<int, string>.Failure("original")
    .Ensure(x => x > 0, "irrelevant");  // Failure("original") — predicate skipped
```

## Combine

Returns `UnitResult<E>.Success()` if all results succeed, otherwise the first failure. Accepts `ReadOnlySpan<Result<T,E>>` to stay zero-alloc at the call site.

```csharp
Span<Result<int, string>> results =
[
    Result<int, string>.Success(1),
    Result<int, string>.Success(2),
    Result<int, string>.Failure("third failed"),
];

UnitResult<string> combined = ResultExtensions.Combine<int, string>(results);
// Failure("third failed")
```

To keep the call fully zero-alloc, use `stackalloc` or a stack-local `Span` — do not pass a heap-allocated array.
