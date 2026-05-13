using BenchmarkDotNet.Attributes;
using OneOf;
using ErrorOr;
using ZaResult = ZeroAlloc.Results.Result<int, string>;
using FlResult = FluentResults.Result<int>;
using OneOfResult = OneOf.OneOf<int, string>;

namespace ZeroAlloc.Results.Benchmarks;

// Compares ZeroAlloc.Results against the three most-cited result-type libraries
// in .NET: OneOf, ErrorOr, FluentResults. Each scenario uses the simplest
// idiomatic call site for each library. Success and failure paths are
// benchmarked separately because the failure path is where wrappers
// historically pay their boxing/allocation costs.
//
// Scenarios:
//   Success_Construct        — build a successful result
//   Failure_Construct        — build a failure result
//   Success_Consume          — check + extract the value on the happy path
//   Failure_Consume          — check + extract the error on the sad path
//   HotLoop_MixedSuccessFail — 100 iterations of div-by-zero-prone work
[MemoryDiagnoser]
[SimpleJob]
public class ResultLibrariesBenchmark
{
    [Params(100)]
    public int Iterations;

    // --- Success path: construct ---

    [Benchmark(Baseline = true, Description = "ZeroAlloc.Results: Success construct")]
    [BenchmarkCategory("Success_Construct")]
    public ZaResult Za_Success_Construct() => ZaResult.Success(42);

    [Benchmark(Description = "OneOf: Success construct")]
    [BenchmarkCategory("Success_Construct")]
    public OneOfResult OneOf_Success_Construct() => OneOfResult.FromT0(42);

    [Benchmark(Description = "ErrorOr: Success construct")]
    [BenchmarkCategory("Success_Construct")]
    public ErrorOr<int> ErrorOr_Success_Construct() => 42;

    [Benchmark(Description = "FluentResults: Success construct")]
    [BenchmarkCategory("Success_Construct")]
    public FlResult FluentResults_Success_Construct() => FluentResults.Result.Ok<int>(42);

    // --- Failure path: construct ---

    [Benchmark(Description = "ZeroAlloc.Results: Failure construct")]
    [BenchmarkCategory("Failure_Construct")]
    public ZaResult Za_Failure_Construct() => ZaResult.Failure("err");

    [Benchmark(Description = "OneOf: Failure construct")]
    [BenchmarkCategory("Failure_Construct")]
    public OneOfResult OneOf_Failure_Construct() => OneOfResult.FromT1("err");

    [Benchmark(Description = "ErrorOr: Failure construct")]
    [BenchmarkCategory("Failure_Construct")]
    public ErrorOr<int> ErrorOr_Failure_Construct() => Error.Validation("E1", "err");

    [Benchmark(Description = "FluentResults: Failure construct")]
    [BenchmarkCategory("Failure_Construct")]
    public FlResult FluentResults_Failure_Construct() => FluentResults.Result.Fail<int>("err");

    // --- Success path: consume ---

    private readonly ZaResult _zaOk = ZaResult.Success(42);
    private readonly OneOfResult _ooOk = OneOfResult.FromT0(42);
    private readonly ErrorOr<int> _eoOk = 42;
    private readonly FlResult _frOk = FluentResults.Result.Ok<int>(42);

    [Benchmark(Description = "ZeroAlloc.Results: Success consume")]
    [BenchmarkCategory("Success_Consume")]
    public int Za_Success_Consume() => _zaOk.IsSuccess ? _zaOk.Value : -1;

    [Benchmark(Description = "OneOf: Success consume")]
    [BenchmarkCategory("Success_Consume")]
    public int OneOf_Success_Consume() => _ooOk.IsT0 ? _ooOk.AsT0 : -1;

    [Benchmark(Description = "ErrorOr: Success consume")]
    [BenchmarkCategory("Success_Consume")]
    public int ErrorOr_Success_Consume() => _eoOk.IsError ? -1 : _eoOk.Value;

    [Benchmark(Description = "FluentResults: Success consume")]
    [BenchmarkCategory("Success_Consume")]
    public int FluentResults_Success_Consume() => _frOk.IsSuccess ? _frOk.Value : -1;

    // --- Failure path: consume ---

    private readonly ZaResult _zaErr = ZaResult.Failure("err");
    private readonly OneOfResult _ooErr = OneOfResult.FromT1("err");
    private readonly ErrorOr<int> _eoErr = Error.Validation("E1", "err");
    private readonly FlResult _frErr = FluentResults.Result.Fail<int>("err");

    [Benchmark(Description = "ZeroAlloc.Results: Failure consume")]
    [BenchmarkCategory("Failure_Consume")]
    public int Za_Failure_Consume() => _zaErr.IsSuccess ? _zaErr.Value : _zaErr.Error.Length;

    [Benchmark(Description = "OneOf: Failure consume")]
    [BenchmarkCategory("Failure_Consume")]
    public int OneOf_Failure_Consume() => _ooErr.IsT0 ? _ooErr.AsT0 : _ooErr.AsT1.Length;

    [Benchmark(Description = "ErrorOr: Failure consume")]
    [BenchmarkCategory("Failure_Consume")]
    public int ErrorOr_Failure_Consume() => _eoErr.IsError ? _eoErr.FirstError.Description.Length : _eoErr.Value;

    [Benchmark(Description = "FluentResults: Failure consume")]
    [BenchmarkCategory("Failure_Consume")]
    public int FluentResults_Failure_Consume() => _frErr.IsSuccess ? _frErr.Value : _frErr.Errors[0].Message.Length;

    // --- Hot loop: mixed success/fail (the realistic workload) ---

    [Benchmark(Description = "ZeroAlloc.Results: HotLoop mixed")]
    [BenchmarkCategory("HotLoop_Mixed")]
    public int Za_HotLoop()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var r = ZaDiv(10, i % 3);
            total += r.IsSuccess ? r.Value : -1;
        }
        return total;
    }

    [Benchmark(Description = "OneOf: HotLoop mixed")]
    [BenchmarkCategory("HotLoop_Mixed")]
    public int OneOf_HotLoop()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var r = OneOfDiv(10, i % 3);
            total += r.IsT0 ? r.AsT0 : -1;
        }
        return total;
    }

    [Benchmark(Description = "ErrorOr: HotLoop mixed")]
    [BenchmarkCategory("HotLoop_Mixed")]
    public int ErrorOr_HotLoop()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var r = ErrorOrDiv(10, i % 3);
            total += r.IsError ? -1 : r.Value;
        }
        return total;
    }

    [Benchmark(Description = "FluentResults: HotLoop mixed")]
    [BenchmarkCategory("HotLoop_Mixed")]
    public int FluentResults_HotLoop()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var r = FluentResultsDiv(10, i % 3);
            total += r.IsSuccess ? r.Value : -1;
        }
        return total;
    }

    private static ZaResult ZaDiv(int n, int d)
        => d == 0 ? ZaResult.Failure("div by zero") : ZaResult.Success(n / d);

    private static OneOfResult OneOfDiv(int n, int d)
        => d == 0 ? OneOfResult.FromT1("div by zero") : OneOfResult.FromT0(n / d);

    private static ErrorOr<int> ErrorOrDiv(int n, int d)
        => d == 0 ? Error.Validation("DIV0", "div by zero") : n / d;

    private static FlResult FluentResultsDiv(int n, int d)
        => d == 0 ? FluentResults.Result.Fail<int>("div by zero") : FluentResults.Result.Ok<int>(n / d);
}
