# ZeroAlloc.Results

[![NuGet](https://img.shields.io/nuget/v/ZeroAlloc.Results.svg)](https://www.nuget.org/packages/ZeroAlloc.Results)
[![Build](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml/badge.svg)](https://github.com/ZeroAlloc-Net/ZeroAlloc.Results/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

ZeroAlloc.Results is a zero-allocation, no-boxing `Result<T, E>` library for .NET 9 with full [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) API parity. All types are `readonly struct` — no heap allocation, no boxing, no GC pressure.

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

All core operations produce **zero heap allocations** (Windows 11, .NET 9.0.14, BenchmarkDotNet v0.13.12).

| Method | Mean | Allocated |
|--------|-----:|----------:|
| `Result<int,string>.Success(42)` | 0.25 ns | **0 B** |
| `Result<int,string>.Failure("err")` | 0.43 ns | **0 B** |
| `.Map(x => x * 2)` | 2.92 ns | **0 B** |
| `.Bind(x => Success(x.ToString()))` | 8.81 ns | **0 B** |
| `.Match(v => v, _ => -1)` | 2.02 ns | **0 B** |
| `Maybe<int>.Some(42)` | 3.66 ns | **0 B** |
| `UnitResult<string>.Success()` | 0.35 ns | **0 B** |

Compare directly against CSharpFunctionalExtensions — all CFE operations allocate a class instance per call:

```bash
dotnet run --project tests/ZeroAlloc.Results.Tests -c Release --filter "*CfeComparisonBenchmarks*"
```

See [docs/performance.md](docs/performance.md) for the full benchmark analysis and zero-allocation design explanation.

## Documentation

| Page | Description |
|------|-------------|
| [Getting Started](docs/getting-started.md) | Install and write your first result pipeline |
| [Types](docs/types.md) | All five result types and when to use each |
| [Combinators](docs/combinators.md) | Map, Bind, Match, Tap, Ensure, Combine |
| [Async](docs/async.md) | ValueTask async variants for all combinators |
| [LINQ](docs/linq.md) | Query syntax with Select and SelectMany |
| [Performance](docs/performance.md) | Zero-alloc design and benchmark results vs CFE |
| [Migration from CFE](docs/migration-from-cfe.md) | Drop-in migration guide from CSharpFunctionalExtensions |

## License

MIT
