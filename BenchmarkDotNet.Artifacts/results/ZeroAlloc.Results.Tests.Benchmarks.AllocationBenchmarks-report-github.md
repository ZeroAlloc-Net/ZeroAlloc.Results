```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8037)
Unknown processor
.NET SDK 10.0.104
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method             | Mean      | Error     | StdDev    | Median    | Allocated |
|------------------- |----------:|----------:|----------:|----------:|----------:|
| Create_Success     | 0.3630 ns | 0.0448 ns | 0.1286 ns | 0.3387 ns |         - |
| Create_Failure     | 0.1575 ns | 0.0461 ns | 0.1285 ns | 0.1228 ns |         - |
| Map_Success        | 1.1308 ns | 0.0620 ns | 0.1579 ns | 1.0850 ns |         - |
| Bind_Chain         | 5.0606 ns | 0.1404 ns | 0.3747 ns | 4.9803 ns |         - |
| Match_Success      | 0.7729 ns | 0.0980 ns | 0.2860 ns | 0.6862 ns |         - |
| Maybe_Some         | 2.4298 ns | 0.0871 ns | 0.1949 ns | 2.3894 ns |         - |
| UnitResult_Success | 0.0453 ns | 0.0320 ns | 0.0646 ns | 0.0001 ns |         - |
