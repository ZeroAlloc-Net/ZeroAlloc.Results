---
id: types
title: Types
slug: /docs/types
description: All five ZeroAlloc.Results types and when to use each.
sidebar_position: 2
---

# Types

ZeroAlloc.Results provides five `readonly struct` types. All are zero-allocation — the struct lives on the stack and carries no heap-allocated wrapper.

## Result

Non-generic pass/fail with an optional string error message.

```csharp
var ok   = Result.Success();
var fail = Result.Failure("something went wrong");

if (fail.IsFailure)
    Console.WriteLine(fail.Error); // "something went wrong"
```

Use when an operation either succeeds or fails with a plain message and there is no return value.

## Result\<T\>

Success with a value of type `T`, failure with a `string` error. The most commonly used type — equivalent to CFE's `Result<T>`.

```csharp
Result<User> user = Result<User>.Success(new User("alice"));
Result<User> err  = Result<User>.Failure("user not found");

// Implicit conversions
Result<int> r = 42;       // success
Result<int> e = "bad id"; // failure
```

## Result\<T, E\>

Fully generic discriminated union. Success value is `T`, failure value is `E`. The core type of the library.

```csharp
Result<User, NotFoundError> result = Result<User, NotFoundError>.Success(user);
Result<User, NotFoundError> err    = Result<User, NotFoundError>.Failure(new NotFoundError(id));

// Implicit conversions
Result<int, string> r = 42;      // success
Result<int, string> e = "oops";  // failure
```

Use when error types are domain-specific structs or enums and you want the compiler to enforce handling.

## UnitResult\<E\>

Operation has no success value but may fail with a typed error. Equivalent to `Result<Unit, E>`.

```csharp
UnitResult<ValidationError> result = UnitResult<ValidationError>.Success();
UnitResult<ValidationError> err    = UnitResult<ValidationError>.Failure(new ValidationError("name is required"));

// Implicit conversion from error
UnitResult<ValidationError> e = new ValidationError("name is required");
```

Use for commands that return nothing on success but carry typed errors on failure.

## Maybe\<T\>

Zero-allocation optional value. Prefer over `Nullable<T>` for reference types.

```csharp
Maybe<User> some = Maybe<User>.Some(user);
Maybe<User> none = Maybe<User>.None;

// Implicit conversion
Maybe<int> m = 42; // Some(42)

bool hasValue = some.HasValue;           // true
int  value    = some.Value;              // 42 (throws if None)
int  safe     = none.GetValueOrDefault(0); // 0
```
