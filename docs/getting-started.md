---
id: getting-started
title: Getting Started
slug: /
description: Install ZeroAlloc.Results and write your first result pipeline in five minutes.
sidebar_position: 1
---

# Getting Started

ZeroAlloc.Results is a zero-allocation `Result<T, E>` library for .NET 9. Every type is a `readonly struct` — no heap allocation on any code path, no GC pressure, no boxing.

## Installation

```bash
dotnet add package ZeroAlloc.Results
```

## Your First Pipeline

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

// 1. Create results
Result<int, string> success = Result<int, string>.Success(42);
Result<int, string> failure = Result<int, string>.Failure("not found");

// 2. Implicit conversion
Result<int, string> r = 42;       // success
Result<int, string> e = "error";  // failure

// 3. Chain operations
var result = GetUserId(input)
    .Ensure(id => id > 0, "id must be positive")
    .Bind(id => LoadUser(id))
    .Map(user => user.Email);

// 4. Extract with Match
string message = result.Match(
    onSuccess: email => $"User email: {email}",
    onFailure: err   => $"Error: {err}");
```

## Choosing a Type

| Scenario | Type |
|---|---|
| Simple pass/fail with a message | `Result` |
| Operation returns a value or a string error | `Result<T>` |
| Operation returns a value or a typed error | `Result<T, E>` |
| Operation succeeds (no value) or fails with typed error | `UnitResult<E>` |
| Value that may or may not be present | `Maybe<T>` |

## Next Steps

- [Types](types.md) — full reference for all five types
- [Combinators](combinators.md) — Map, Bind, Match, Tap, Ensure, Combine
- [Async](async.md) — ValueTask async pipelines
- [LINQ](linq.md) — query syntax support
