---
id: linq
title: LINQ Query Syntax
slug: /docs/linq
description: Use C# query syntax with Result<T,E> via Select and SelectMany.
sidebar_position: 5
---

# LINQ Query Syntax

`ResultLinqExtensions` enables C# query syntax for `Result<T, E>` via `Select` (= `Map`) and `SelectMany` (= `Bind`).

```csharp
using ZeroAlloc.Results.Extensions;
```

## Select (Map)

```csharp
var doubled = from x in Result<int, string>.Success(5)
              select x * 2;
// Success(10)
```

## SelectMany (Bind)

Chain multiple result-returning operations. The pipeline short-circuits at the first failure.

```csharp
var result =
    from userId  in ParseUserId(input)
    from user    in LoadUser(userId)
    from profile in LoadProfile(user)
    select profile.DisplayName;
```

Equivalent to:

```csharp
var result = ParseUserId(input)
    .Bind(id      => LoadUser(id))
    .Bind(user    => LoadProfile(user))
    .Map(profile  => profile.DisplayName);
```

## Short-circuit behaviour

If any step fails, subsequent steps are not executed:

```csharp
var secondCalled = false;

var result =
    from user in Result<string, string>.Failure("no user")
    from greeting in (secondCalled = true, Result<string, string>.Success("hi")).Item2
    select greeting;

// secondCalled == false — the lambda was never invoked
// result.IsFailure == true, result.Error == "no user"
```
