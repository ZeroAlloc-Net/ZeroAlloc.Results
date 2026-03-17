using BenchmarkDotNet.Attributes;
using ZeroAlloc.Results;
using ZeroAlloc.Results.Extensions;

namespace ZeroAlloc.Results.Tests.Benchmarks;

/// <summary>
/// Verifies zero heap allocation for all core Result operations.
/// All Gen0 values MUST be 0 — any non-zero value is a regression.
/// Run with: dotnet run --project tests/ZeroAlloc.Results.Tests -c Release
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
public class AllocationBenchmarks
{
    [Benchmark]
    public Result<int, string> Create_Success() =>
        Result<int, string>.Success(42);

    [Benchmark]
    public Result<int, string> Create_Failure() =>
        Result<int, string>.Failure("error");

    [Benchmark]
    public Result<int, string> Map_Success() =>
        Result<int, string>.Success(5).Map(x => x * 2);

    [Benchmark]
    public Result<string, string> Bind_Chain() =>
        Result<int, string>.Success(5)
            .Bind(x => Result<string, string>.Success(x.ToString()));

    [Benchmark]
    public int Match_Success() =>
        Result<int, string>.Success(42).Match(v => v, _ => -1);

    [Benchmark]
    public Maybe<int> Maybe_Some() => Maybe<int>.Some(42);

    [Benchmark]
    public UnitResult<string> UnitResult_Success() => UnitResult<string>.Success();
}
