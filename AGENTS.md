# AGENTS.md: ZeroAlloc.Results

`ZeroAlloc.Results` is a single NuGet package: railway-style `Result`, `Result<T>`, `Result<T, E>`,
`UnitResult<E>` and `Maybe<T>` as `readonly struct`s, with combinators, LINQ query syntax and
`ValueTask` variants. It is a pure runtime library (no source generator) for net10.0 and net11.0.
Success and failure both allocate 0 B, and it must work under NativeAOT.

## Layout

- `src/ZeroAlloc.Results` is the only shipped project. `Result1.cs` and `Result2.cs` are the
  arity-1 and arity-2 partials of the same family, not duplicates (`MA0048` is suppressed for this).
- `src/ZeroAlloc.Results/PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` lock the public
  surface. `RS0016` and `RS0017` are errors, so a new or removed public symbol fails the build until
  `PublicAPI.Unshipped.txt` is updated. Never edit `PublicAPI.Shipped.txt` by hand: the release
  workflow moves entries there.
- `tests/ZeroAlloc.Results.Tests` is an `Exe` that holds the xunit tests and, under `Benchmarks/`,
  two embedded BenchmarkDotNet classes (`AllocationBenchmarks`, `CfeComparisonBenchmarks`). It has
  its own top-level-statements entry point, so `GenerateProgramFile` is off.
- `benchmarks/ZeroAlloc.Results.Benchmarks` compares against OneOf, ErrorOr and FluentResults.
  It is not shipped.
- `samples/ZeroAlloc.Results.AotSmoke` is the NativeAOT smoke app. It is not shipped.

## Build and test

Prerequisites: `global.json` pins SDK 11.0.100 (`rollForward: latestMinor`). Running the tests needs
the .NET 10 and .NET 11 runtimes, one per test target framework (`net10.0`, `net11.0`). The benchmarks
target `net10.0` and need the .NET 10 runtime.
`dotnet tool restore` installs GitVersion.

CI job `build` in `.github/workflows/ci.yml`:

```bash
dotnet tool restore
VERSION=$(dotnet dotnet-gitversion /output json /showvariable SemVer)
dotnet restore ZeroAlloc.Results.slnx
dotnet build ZeroAlloc.Results.slnx --configuration Release --no-restore
dotnet test ZeroAlloc.Results.slnx --configuration Release --no-build --verbosity normal --logger "trx;LogFilePrefix=test-results" --collect "XPlat Code Coverage" --settings coverage.runsettings --results-directory TestResults
```

CI then runs a coverage gate (the `ci-baseline` `coverage-gate` action) on
`TestResults/**/coverage.cobertura.xml`. That floor is not reproduced by the commands above. The
`codecov.yml` project check allows a 1% drop.

CI job `aot-smoke` (needs `clang` and `zlib1g-dev`):

```bash
dotnet restore samples/ZeroAlloc.Results.AotSmoke/ZeroAlloc.Results.AotSmoke.csproj
dotnet publish samples/ZeroAlloc.Results.AotSmoke/ZeroAlloc.Results.AotSmoke.csproj -r linux-x64 -c Release -o ./aot-out
./aot-out/ZeroAlloc.Results.AotSmoke
```

CI job `api-compat` (shared workflow) checks the public API against the last release. CI jobs
`smoke` (every PR and push, shared `benchmark-smoke.yml`) and `full` (manual, `benchmarks.yml`)
run the benchmark project. `full` runs:

```bash
dotnet build benchmarks/ZeroAlloc.Results.Benchmarks -c Release
dotnet run --project benchmarks/ZeroAlloc.Results.Benchmarks -c Release --no-build -- --filter "${FILTER:-*}"
```

Inner loop, verified locally, not a CI job:

```bash
dotnet build ZeroAlloc.Results.slnx -c Release
dotnet test tests/ZeroAlloc.Results.Tests -c Release --no-build -f net10.0
dotnet test tests/ZeroAlloc.Results.Tests -c Release --no-build -f net11.0 --filter "FullyQualifiedName~CombinatorAllocationTests"
```

## Conventions and gotchas

- The library and the test project target `net10.0;net11.0`; the benchmark project targets only
  `net10.0`. BenchmarkDotNet 0.15.8 cannot run on net11.0 (issue #191), so the benchmarks embedded
  in the test project run on net10.0 only: use `dotnet run -f net10.0`.
- The AOT smoke targets net11.0 and promotes `IL2026`, `IL2067`, `IL2075`, `IL2091`, `IL3050` and
  `IL3051` to errors. The library is pure runtime, so a reflection call anywhere in a `Result` or
  `Maybe` path breaks it.
- `TreatWarningsAsErrors` is on for every project, with `Meziantou.Analyzer` on all non-netstandard
  targets. `src` has `AllowUnsafeBlocks=false`.
- Allocation tests assert 0 B with `AllocationGate.AssertBudget` (ZeroAlloc.TestHelpers) for both the
  success and the failure path. Keep that shape for a new combinator.
- Packing needs a supplied version: `Directory.Build.props` fails `dotnet pack` with `ZAVERSION001`
  or `ZAVERSION002` unless `-p:Version` is passed to every build step as well as the pack. Build,
  test and publish are unaffected.

<!-- org-core:start sha256=8faf417fa86f11bd8ad9bc98dfa4683fba44e440291e84c6cb9b0ec3ca0c0e7e (generated from docker-nuc scripts/agents-md/org-core-zeroalloc.md; do not edit here) -->
## Org rules

These rules hold in every ZeroAlloc-Net repo. Each library makes two promises: it works under
NativeAOT and trimming, and its hot path allocates nothing. The 2026-10-11 sweep found 16 of 28 repos
allocating on the hot path, most with docs claiming 0 B.

### Hot path

The operation a consumer runs repeatedly: `Send`, a cache hit, `Validate`, `Map`, `Serialize`, a
resilience call, a state transition, an append, a worker poll, including the idle poll. Startup and
registration are not hot path. For a source generator, the hot path is the code it emits: judge the
snapshots and the generated output.

### Allocation traps the sweep found

| Pattern | Instead |
|---|---|
| `async ValueTask` that only awaits and returns | `return new ValueTask<T>(inner.XAsync(ct));` when the method only forwards a `Task<T>` (return a forwarded `ValueTask<T>` directly). The wrapper stores a synchronous throw in the returned task; the direct return lets it, and the cancellation status of the callee, reach the caller. Keep the `async` wrapper when callers depend on either |
| `Task.WhenAll` over tasks that already completed | skip the await only when every task has `IsCompletedSuccessfully` (`Status == TaskStatus.RanToCompletion` on `netstandard2.0`); otherwise `await Task.WhenAll`, which also reports faults of completed tasks |
| `TagList` or log `params object[]` built before `Enabled`/`IsEnabled` | check first, or `LoggerMessage.Define` |
| event args allocated before checking for subscribers | copy the handler to a local, check, then allocate |
| `new Serializer()` / `new Utf8JsonWriter()` per call | cache stateless instances, reuse writers with `Reset` |
| `GetString()` / `ToString()` round-trips in JSON converters | parse and format through spans |
| `ToArray()` / `ToList()` copies between layers | pass `ReadOnlyMemory<T>` or spans, copy once at most |
| LINQ in emitted code | emit `for` loops |
| `yield` iterator behind a public `GetEnumerator` | public struct enumerator, from the first release |
| `Enum.IsDefined(typeof(E), v)` with a boxed value, `decimal.GetBits(v)` to read the scale | `Enum.IsDefined<E>(v)` when `v` has type `E` (.NET 5 or later, so not on .NET Framework, `netstandard2.0`, `netstandard2.1` or .NET Core 3.1); `v.Scale` for a scale read only (.NET 7 or later, never for the coefficient or sign). Keep the old call on older targets |
| configuration parsed or read per call | parse once and cache |
| an `AsyncLocal` write per call | pass state explicitly |
| a capturing lambda or method-group delegate per call | cache the delegate in a field, or a static lambda |
| a per-key object that is never removed | remove on release; otherwise it is a leak |

Inherent allocations are fine when documented: the consumer's own result, an exception on a failure
path, a one-time cache fill.

### Tests and benchmarks

- Every hot path has an allocation gate test (`AllocationGate` from ZeroAlloc.TestHelpers) that measures
  the whole public call, including any state setup.
- Budgets are exact. If a path allocates, assert that number and comment what it is.
- Test the generated code, not a hand-written stand-in, and cover every shape: each attribute, nested
  collections, and the asynchronous case with a fake that returns pending tasks.
- Benchmarks run in CI through the shared `benchmark-smoke.yml` and target the library's LTS
  frameworks. The benchmark body allocates nothing besides the library itself. Run locally with
  `--inProcess`.
- Publish exact allocations and same-run ratios against a `Baseline`, never absolute timings. Run with
  `--launchCount 3`, pin CPUs with `taskset`, give the spread, and label every table.

### NativeAOT and trimming

- Packable runtime projects, and projects consumers publish, set `IsAotCompatible=true` on modern
  target frameworks. Analyzers and source generators target `netstandard2.0`.
- No reflection or runtime code generation on a supported path. An opt-in reflection fallback carries
  honest `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`. Every suppression must be true;
  remove dead ones.
- The AOT smoke sample publishes with `TreatWarningsAsErrors=true`. If a third-party warning really
  blocks it, use a narrow, justified suppression for that warning. `WarningsNotAsErrors` selects codes,
  not assemblies, so it would also downgrade the same warning from generated code.
- The smoke runs the hot path of every package in the repo. A package without a smoke has unknown AOT
  status. Adapters run against real services in CI service containers; publish-only is a last resort.
  App and template smokes make authenticated requests.

### Docs and shipping

- Docs match measurements. After a hot-path change, re-measure and update `docs/performance.md` and the
  README. Never write "0 B" without a test that asserts it.
- New public API is `feat:`, even for a performance change. Public API changes are additive: never add
  an optional parameter to a shipped signature. A breaking end state goes in a `next-major` issue.
- A behaviour change gets a `## Behaviour change` section in the PR body.
- Conventional Commits with release-please. Subjects start lowercase, body lines stay within 100
  characters, and a body never contains nested parentheses: release-please drops the whole commit.
- Local packs use `9.9.9-local.<sha>`. After a release, verify the package on NuGet.
- Fix review findings in the PR; file an issue only for what cannot go in it.

Full rules, template wiring lessons and the reasons behind each rule: https://github.com/ZeroAlloc-Net/.github/blob/main/AGENTS.md
<!-- org-core:end -->
