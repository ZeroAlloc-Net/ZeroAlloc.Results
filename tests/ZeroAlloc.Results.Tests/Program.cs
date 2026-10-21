using BenchmarkDotNet.Running;
using ZeroAlloc.Results.Tests.Benchmarks;

var switcher = BenchmarkSwitcher.FromTypes([
    typeof(AllocationBenchmarks),
    typeof(CfeComparisonBenchmarks)
]);

// Run(args), not RunAll(): RunAll ignores the command line, so --filter, --launchCount and the
// artifacts path would be dropped.
switcher.Run(args);
