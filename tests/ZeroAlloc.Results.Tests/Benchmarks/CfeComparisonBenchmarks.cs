using BenchmarkDotNet.Attributes;
using CSharpFunctionalExtensions;
using ZeroAlloc.Results.Extensions;
using ZA = ZeroAlloc.Results;
using CfeResult = CSharpFunctionalExtensions.Result;
using CfeResultT = CSharpFunctionalExtensions.Result<int, string>;
using CfeResultStr = CSharpFunctionalExtensions.Result<string, string>;

namespace ZeroAlloc.Results.Tests.Benchmarks;

/// <summary>
/// Head-to-head allocation comparison: ZeroAlloc.Results vs CSharpFunctionalExtensions.
/// ZA_* benchmarks show Allocated = "-"; CFE_* benchmarks show heap allocation cost.
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CfeComparisonBenchmarks
{
    // ── Create Success ────────────────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Create_Success")]
    public ZA.Result<int, string> ZA_Create_Success() =>
        ZA.Result<int, string>.Success(42);

    [Benchmark, BenchmarkCategory("Create_Success")]
    public CfeResultT CFE_Create_Success() =>
        CfeResult.Success<int, string>(42);

    // ── Create Failure ────────────────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Create_Failure")]
    public ZA.Result<int, string> ZA_Create_Failure() =>
        ZA.Result<int, string>.Failure("error");

    [Benchmark, BenchmarkCategory("Create_Failure")]
    public CfeResultT CFE_Create_Failure() =>
        CfeResult.Failure<int, string>("error");

    // ── Map ───────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Map")]
    public ZA.Result<int, string> ZA_Map() =>
        ZA.Result<int, string>.Success(5).Map(x => x * 2);

    [Benchmark, BenchmarkCategory("Map")]
    public CfeResultT CFE_Map() =>
        CfeResult.Success<int, string>(5).Map(x => x * 2);

    // ── Bind ──────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Bind")]
    public ZA.Result<string, string> ZA_Bind() =>
        ZA.Result<int, string>.Success(5)
            .Bind(x => ZA.Result<string, string>.Success(x.ToString()));

    [Benchmark, BenchmarkCategory("Bind")]
    public CfeResultStr CFE_Bind() =>
        CfeResult.Success<int, string>(5)
            .Bind(x => CfeResult.Success<string, string>(x.ToString()));

    // ── Match ─────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Match")]
    public int ZA_Match() =>
        ZA.Result<int, string>.Success(42).Match(v => v, _ => -1);

    [Benchmark, BenchmarkCategory("Match")]
    public int CFE_Match() =>
        CfeResult.Success<int, string>(42).Match(v => v, _ => -1);

    // ── Chain (Map + Bind + Match) ────────────────────────────────────────

    [Benchmark(Baseline = true), BenchmarkCategory("Chain")]
    public int ZA_Chain() =>
        ZA.Result<int, string>.Success(5)
            .Map(x => x * 2)
            .Bind(x => ZA.Result<string, string>.Success(x.ToString()))
            .Match(s => s.Length, _ => -1);

    [Benchmark, BenchmarkCategory("Chain")]
    public int CFE_Chain() =>
        CfeResult.Success<int, string>(5)
            .Map(x => x * 2)
            .Bind(x => CfeResult.Success<string, string>(x.ToString()))
            .Match(s => s.Length, _ => -1);
}
