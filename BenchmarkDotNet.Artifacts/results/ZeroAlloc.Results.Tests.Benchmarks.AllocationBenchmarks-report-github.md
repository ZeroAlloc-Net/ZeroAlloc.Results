```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8037)
Unknown processor
.NET SDK 10.0.104
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method             | Mean      | Error     | StdDev    | Median    | Allocated |
|------------------- |----------:|----------:|----------:|----------:|----------:|
| Create_Success     | 0.2573 ns | 0.1101 ns | 0.3247 ns | 0.0467 ns |         - |
| Create_Failure     | 0.3494 ns | 0.0422 ns | 0.1177 ns | 0.3329 ns |         - |
| Map_Success        | 1.5741 ns | 0.1154 ns | 0.3178 ns | 1.5058 ns |         - |
| Bind_Chain         | 7.6801 ns | 0.7018 ns | 2.0692 ns | 7.0595 ns |         - |
| Match_Success      | 2.2700 ns | 0.6099 ns | 1.7983 ns | 2.4916 ns |         - |
| Maybe_Some         | 3.4659 ns | 0.2167 ns | 0.6321 ns | 3.4697 ns |         - |
| UnitResult_Success | 0.3485 ns | 0.0927 ns | 0.2690 ns | 0.2767 ns |         - |
