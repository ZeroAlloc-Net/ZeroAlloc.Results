```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.8037)
Unknown processor
.NET SDK 10.0.104
  [Host]     : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.14 (9.0.1426.11910), X64 RyuJIT AVX2


```
| Method             | Categories     | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Allocated | Alloc Ratio |
|------------------- |--------------- |----------:|----------:|----------:|----------:|------:|--------:|----------:|------------:|
| ZA_Bind            | Bind           | 5.0487 ns | 0.2927 ns | 0.8493 ns | 4.7736 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Bind           | Bind           | 4.6908 ns | 0.1311 ns | 0.3216 ns | 4.6293 ns |  0.93 |    0.15 |         - |          NA |
|                    |                |           |           |           |           |       |         |           |             |
| ZA_Chain           | Chain          | 2.2823 ns | 0.0833 ns | 0.1460 ns | 2.2694 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Chain          | Chain          | 2.4507 ns | 0.1260 ns | 0.3553 ns | 2.3337 ns |  1.08 |    0.15 |         - |          NA |
|                    |                |           |           |           |           |       |         |           |             |
| ZA_Create_Failure  | Create_Failure | 0.3025 ns | 0.0444 ns | 0.0947 ns | 0.2877 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Create_Failure | Create_Failure | 1.4400 ns | 0.0664 ns | 0.1667 ns | 1.4237 ns |  5.34 |    1.97 |         - |          NA |
|                    |                |           |           |           |           |       |         |           |             |
| ZA_Create_Success  | Create_Success | 0.3334 ns | 0.0442 ns | 0.1110 ns | 0.2958 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Create_Success | Create_Success | 2.8947 ns | 0.1121 ns | 0.3234 ns | 2.8011 ns |  9.73 |    3.37 |         - |          NA |
|                    |                |           |           |           |           |       |         |           |             |
| ZA_Map             | Map            | 1.0944 ns | 0.0613 ns | 0.1210 ns | 1.1038 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Map            | Map            | 1.4760 ns | 0.0731 ns | 0.2075 ns | 1.4420 ns |  1.37 |    0.27 |         - |          NA |
|                    |                |           |           |           |           |       |         |           |             |
| ZA_Match           | Match          | 0.3667 ns | 0.0471 ns | 0.1358 ns | 0.3209 ns |  1.00 |    0.00 |         - |          NA |
| CFE_Match          | Match          | 0.6816 ns | 0.0566 ns | 0.1624 ns | 0.6302 ns |  2.07 |    0.77 |         - |          NA |
