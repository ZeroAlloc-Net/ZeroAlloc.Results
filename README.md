# ZeroAlloc.Results

[![NuGet](https://img.shields.io/nuget/v/ZeroAlloc.Results.svg)](https://www.nuget.org/packages/ZeroAlloc.Results)
[![Build](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![AOT](https://img.shields.io/badge/AOT--Compatible-passing-brightgreen)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
[![GitHub Sponsors](https://img.shields.io/github/sponsors/MarcelRoozekrans?style=flat&logo=githubsponsors&color=ea4aaa&label=Sponsor)](https://github.com/sponsors/MarcelRoozekrans)

ZeroAlloc.Results is a zero-allocation, no-boxing `Result<T, E>` library for .NET 9. All types are `readonly struct` — no heap allocation, no boxing, no GC pressure.

## Install

```bash
dotnet add package ZeroAlloc.Results
```

## Quick Example

```csharp
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

// Create
Result<int, string> ok = Result<int, string>.Success(42);
Result<int, string> fail = Result<int, string>.Failure("not found");

// Or via implicit conversion
Result<int, string> r = 42;       // success
Result<int, string> e = "error";  // failure

// Chain with Map, Bind, Ensure
var result = GetUser(id)
    .Ensure(u => u.IsActive, "user is inactive")
    .Map(u => u.Email)
    .Bind(email => SendWelcome(email));

// Match to extract
string message = result.Match(
    onSuccess: email => $"Sent to {email}",
    onFailure: err  => $"Failed: {err}");

// LINQ query syntax
var greeting =
    from user    in GetUser(id)
    from profile in GetProfile(user)
    select $"Hello, {profile.Name}";

// Async pipelines with ValueTask
var response = await GetUser(id)
    .MapAsync(async u  => await LoadPermissions(u))
    .BindAsync(async p => await BuildToken(p));
```

## Types

| Type | Success | Error | Use case |
|------|---------|-------|----------|
| `Result` | — | `string` | simple pass/fail |
| `Result<T>` | `T` | `string` | most common |
| `Result<T, E>` | `T` | `E` | fully generic |
| `UnitResult<E>` | — | `E` | typed error, no value |
| `Maybe<T>` | `T` | — | optional value |

## API

| Method | Description |
|--------|-------------|
| `Map(T→U)` | Transform the success value |
| `MapError(E→F)` | Transform the error value |
| `Bind(T→Result<U,E>)` | Chain result-returning functions |
| `Match(onSuccess, onFailure)` | Extract a value from either branch |
| `Tap(T→void)` | Side-effect on success, pass through |
| `TapError(E→void)` | Side-effect on failure, pass through |
| `Ensure(T→bool, E)` | Validate success value |
| `Combine(Span<Result<T,E>>)` | Merge multiple results, zero-alloc |
| `*Async` | `ValueTask`-based variant of each combinator |

## Performance

ZeroAlloc.Results is the **only result library in .NET with 0 B allocation on every path** — including failure construction. .NET 10.0.7, i9-12900HK, BenchmarkDotNet v0.15.4.

| Scenario | ZeroAlloc.Results | OneOf | ErrorOr | FluentResults |
|---|---:|---:|---:|---:|
| Success construct | 0.4 ns / 0 B | 0.5 ns / 0 B | 0.0 ns / 0 B | 87 ns / **112 B** |
| Failure construct | 0.3 ns / 0 B | 0.9 ns / 0 B | 63 ns / **184 B** | 87 ns / **272 B** |
| Failure consume | 0.4 ns / 0 B | 0.9 ns / 0 B | 2.6 ns / 0 B | 214 ns / **240 B** |
| Hot loop (100 iter, 1-in-3 fail) | **183 ns / 0 B** | 202 ns / 0 B | 7,693 ns / 6,256 B | 39,450 ns / 25,968 B |

On the realistic mixed-success/failure workload, ZeroAlloc.Results is **1.1× faster than OneOf**, **42× faster than ErrorOr**, and **216× faster than FluentResults** — with zero allocation while the latter two allocate per-failure.

Head-to-head vs [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions): **1.1–8.7× faster** depending on operation; 0 B both.

See [docs/performance.md](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/performance.md) for full methodology, all scenarios, and analysis.

## Documentation

| Page | Description |
|------|-------------|
| [Getting Started](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/getting-started.md) | Install and write your first result pipeline |
| [Types](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/types.md) | All five result types and when to use each |
| [Combinators](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/combinators.md) | Map, Bind, Match, Tap, Ensure, Combine |
| [Async](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/async.md) | ValueTask async variants for all combinators |
| [LINQ](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/linq.md) | Query syntax with Select and SelectMany |
| [Performance](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/blob/main/docs/performance.md) | Zero-alloc design and benchmark results |

## License

MIT
