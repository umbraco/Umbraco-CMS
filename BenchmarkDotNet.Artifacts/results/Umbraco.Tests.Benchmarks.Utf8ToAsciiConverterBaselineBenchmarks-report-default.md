
BenchmarkDotNet v0.15.6, Linux Ubuntu 25.10 (Questing Quokka)
Intel Xeon CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4


 Method                 | Mean            | Error         | StdDev        | Rank | Gen0     | Gen1     | Gen2     | Allocated |
----------------------- |----------------:|--------------:|--------------:|-----:|---------:|---------:|---------:|----------:|
 Tiny_Ascii             |        82.81 ns |      0.402 ns |      0.314 ns |    2 |   0.0027 |        - |        - |      48 B |
 Tiny_Mixed             |        71.05 ns |      0.225 ns |      0.176 ns |    1 |   0.0027 |        - |        - |      48 B |
 Small_Ascii            |       695.75 ns |      4.394 ns |      3.669 ns |    3 |   0.0124 |        - |        - |     224 B |
 Small_Mixed            |       686.54 ns |      8.868 ns |      8.295 ns |    3 |   0.0124 |        - |        - |     224 B |
 Medium_Ascii           |     5,994.68 ns |     32.905 ns |     30.779 ns |    4 |   0.4730 |        - |        - |    8240 B |
 Medium_Mixed           |     7,116.65 ns |     27.489 ns |     22.955 ns |    5 |   0.4730 |        - |        - |    8264 B |
 Large_Ascii            |   593,733.29 ns |  2,040.378 ns |  1,703.808 ns |    7 | 249.0234 | 249.0234 | 249.0234 |  819332 B |
 Large_Mixed            | 1,066,297.43 ns |  8,507.650 ns |  7,958.061 ns |    8 | 248.0469 | 248.0469 | 248.0469 |  823523 B |
 Large_WorstCase        | 2,148,169.56 ns | 16,455.374 ns | 15,392.367 ns |    9 | 246.0938 | 246.0938 | 246.0938 | 1024125 B |
 CharArray_Medium_Mixed |     7,357.24 ns |     59.719 ns |     55.861 ns |    6 |   0.5951 |   0.0076 |        - |   10336 B |
