using System;
using BenchmarkDotNet.Attributes;
using ZeroAlloc.Results;

namespace ZeroAlloc.Results.Benchmarks;

// Compares the allocation / throughput cost of propagating failure via
// Result<T, E> (the zero-allocation path) against throwing-and-catching
// exceptions (the baseline). Both variants are called in a tight loop so
// the fixed per-iteration overhead dominates.
//
// The zero-allocation claim: Result<T, E> wraps value and error in a
// readonly struct, so the failure path allocates 0 B/op.
[MemoryDiagnoser]
[SimpleJob]
public class ResultVsExceptionBenchmark
{
    [Params(100)]
    public int Iterations;

    [Benchmark(Baseline = true, Description = "throw/catch")]
    public int ThrowCatch()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            try { total += DivThrow(10, i % 3); }
            catch (DivideByZeroException) { total += -1; }
        }
        return total;
    }

    [Benchmark(Description = "Result<int, string>")]
    public int Result()
    {
        var total = 0;
        for (var i = 0; i < Iterations; i++)
        {
            var r = DivResult(10, i % 3);
            total += r.IsSuccess ? r.Value : -1;
        }
        return total;
    }

    private static int DivThrow(int num, int denom)
    {
        if (denom == 0) throw new DivideByZeroException();
        return num / denom;
    }

    private static Result<int, string> DivResult(int num, int denom)
    {
        if (denom == 0) return Result<int, string>.Failure("div by zero");
        return Result<int, string>.Success(num / denom);
    }
}
