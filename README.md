# ZeroAlloc.Results

[![NuGet](https://img.shields.io/nuget/v/ZeroAlloc.Results.svg)](https://www.nuget.org/packages/ZeroAlloc.Results)
[![Build](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/ZeroAlloc-Net/ZeroAlloc.Results/graph/badge.svg)](https://codecov.io/gh/ZeroAlloc-Net/ZeroAlloc.Results)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/LICENSE)
[![AOT](https://img.shields.io/badge/AOT--Compatible-passing-brightgreen)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/MarcelRoozekrans?style=flat&logo=githubsponsors&color=ea4aaa&label=Sponsor)](https://github.com/sponsors/MarcelRoozekrans)

Railway-style `Result<T, E>` for .NET 8, 9 and 10: errors as values instead of exceptions, 0 B allocated on success and failure alike, NativeAOT-ready.

## Why ZeroAlloc.Results

- **0 B per call, on both paths.** Every type is a `readonly struct`. Allocation tests assert 0 B for `Map`, `Bind`, `Match`, `Tap`, `Ensure`, `Combine` and LINQ query syntax, for success and failure, and for the `ValueTask` combinators when they complete synchronously.
- **Faster when things fail.** On a hot loop where one call in three fails, it runs 4.1× faster than ErrorOr and 41× faster than FluentResults in the same benchmark run, at parity with OneOf, and allocates nothing.
- **Works under NativeAOT and trimming.** No reflection. The package is marked AOT-compatible, and CI publishes and runs a NativeAOT sample with trim and AOT warnings promoted to errors.
- **A familiar API.** `Map`, `Bind`, `Match`, `Tap` and `Ensure`, LINQ query syntax and `ValueTask` async variants. Coming from CSharpFunctionalExtensions, most code moves over by swapping the `using`.

## Install

```bash
dotnet add package ZeroAlloc.Results
```

## Quick start

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

Console.WriteLine(Describe("42"));  // Welcome, age 42
Console.WriteLine(Describe("12"));  // Rejected: too young
Console.WriteLine(Describe("abc")); // Rejected: not a number

static string Describe(string input) =>
    ParseAge(input)
        .Ensure(static age => age >= 18, "too young")
        .Map(static age => $"Welcome, age {age}")
        .Match(static text => text, static error => $"Rejected: {error}");

// A value converts to success, an error converts to failure.
static Result<int, string> ParseAge(string input) =>
    int.TryParse(input, out var age) ? age : "not a number";
```

No exception is thrown: the first failure skips the remaining steps and lands in `Match`.

## What you get

- **Five result shapes:** pass/fail, a value or a string error, a value or a typed error, a typed error with no value, and an optional value (`Maybe<T>`).
- **Railway-style combinators:** transform, chain, validate, inspect and unwrap a result without `if` checks.
- **Batch checks:** combine many results into one that reports the first failure.
- **LINQ query syntax:** chain dependent steps with `from … in … select`.
- **Async pipelines:** `ValueTask`-based variants, plus `Map` and `Bind` that continue straight onto a `ValueTask<Result<T, E>>`.
- **Typed errors:** use your own error structs or enums, so failures carry data rather than strings.
- **Generic failure construction:** pipeline code can build a failed result for a response type it only knows as a type parameter, without reflection.

## Performance

.NET 10.0.13, BenchmarkDotNet v0.15.8, all libraries in the same run. Allocations are exact; time is the ratio against ZeroAlloc.Results.

| Scenario | ZeroAlloc.Results | OneOf | ErrorOr | FluentResults |
|---|---:|---:|---:|---:|
| Failure construct | 0 B | 0 B | **88 B** | **272 B** |
| Failure consume | 0 B | 0 B | 0 B | **240 B** |
| Hot loop (100 iter, 1-in-3 fail), time | **1.00** | 1.02 | 4.14 | 41.4 |
| Hot loop, allocated | **0 B** | 0 B | 2,992 B | 25,968 B |

The [performance page](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/performance.md) has every scenario, a comparison with CSharpFunctionalExtensions, a Result versus throw/catch benchmark and how to run them yourself.

## Learn more

- **Documentation:** [results.zeroalloc.net](https://results.zeroalloc.net): [types](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/types.md), [combinators](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/combinators.md), [async](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/async.md), [LINQ](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/linq.md) and [migrating from CSharpFunctionalExtensions](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/migration-from-cfe.md).
- **Related ZeroAlloc packages:**
  - [ZeroAlloc.Mediator](https://github.com/ZeroAlloc-Net/ZeroAlloc.Mediator) ([docs](https://mediator.zeroalloc.net)): its validation and authorization behaviors return a failed `Result<T, E>` instead of throwing.
  - [ZeroAlloc.Pipeline](https://github.com/ZeroAlloc-Net/ZeroAlloc.Pipeline) ([docs](https://pipeline.zeroalloc.net)): the source-generated behavior pipeline that Mediator's behaviors run on.
  - [ZeroAlloc.Validation](https://github.com/ZeroAlloc-Net/ZeroAlloc.Validation) ([docs](https://validation.zeroalloc.net)): source-generated validators; through Mediator, a failed validation becomes a `Result<T, ValidationError>`.
  - [ZeroAlloc.Authorization](https://github.com/ZeroAlloc-Net/ZeroAlloc.Authorization) ([docs](https://authorization.zeroalloc.net)): authorization policies return a `UnitResult<AuthorizationFailure>`.
- **Contributing:** issues and pull requests are welcome on [GitHub](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/issues).
- **License:** [MIT](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/LICENSE).
