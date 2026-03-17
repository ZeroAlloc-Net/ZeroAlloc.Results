using BenchmarkDotNet.Running;
using ZeroAlloc.Results.Tests.Benchmarks;

var switcher = BenchmarkSwitcher.FromTypes([
    typeof(AllocationBenchmarks),
    typeof(CfeComparisonBenchmarks)
]);

switcher.RunAll();
