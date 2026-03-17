---
id: async
title: Async
slug: /docs/async
description: ValueTask-based async variants for all combinators.
sidebar_position: 4
---

# Async

Every combinator has a `ValueTask`-based async variant in `ResultAsyncExtensions`. The `Result` struct itself remains zero-allocation; async state machines do allocate on first suspension.

```csharp
using ZeroAlloc.Results.Extensions;
```

## MapAsync

```csharp
var result = await Result<int, string>.Success(5)
    .MapAsync(async x =>
    {
        await Task.Delay(1);
        return x * 2;
    });
// Success(10)
```

## BindAsync

```csharp
var result = await Result<int, string>.Success(userId)
    .BindAsync(async id => await LoadUserAsync(id));
```

## TapAsync / TapErrorAsync

```csharp
await result
    .TapAsync(async v => await auditLog.WriteAsync($"success: {v}"))
    .TapErrorAsync(async e => await alerting.NotifyAsync(e));
```

## MatchAsync

```csharp
string message = await result.MatchAsync(
    async v => await FormatSuccess(v),
    async e => await FormatError(e));
```

## Pipeline continuations

Chain combinators directly onto `ValueTask<Result<T,E>>`:

```csharp
var response = await GetUserAsync(id)       // ValueTask<Result<User, Error>>
    .Map(u => u.Email)                      // sync Map on async result
    .Bind(email => ValidateEmail(email));   // sync Bind on async result
```

## Choosing ValueTask vs Task

ZeroAlloc.Results uses `ValueTask` throughout because:
- `ValueTask` avoids a `Task` heap allocation when the operation completes synchronously
- Hot paths (cache hits, validation short-circuits) pay zero allocation cost
- `Task`-returning lambdas are accepted via interop overloads on all async methods
