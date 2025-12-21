```

BenchmarkDotNet v0.15.6, Linux Ubuntu 25.10 (Questing Quokka)
Intel Xeon CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4


```
| Method            | Mean             | Error          | StdDev         | Rank | Gen0     | Gen1     | Gen2     | Allocated |
|------------------ |-----------------:|---------------:|---------------:|-----:|---------:|---------:|---------:|----------:|
| Tiny_Ascii        |         6.756 ns |      0.1042 ns |      0.0974 ns |    1 |        - |        - |        - |         - |
| Tiny_Mixed        |         6.554 ns |      0.0153 ns |      0.0143 ns |    1 |        - |        - |        - |         - |
| Small_Ascii       |         8.132 ns |      0.0271 ns |      0.0253 ns |    2 |        - |        - |        - |         - |
| Small_Mixed       |       308.895 ns |      0.6975 ns |      0.6525 ns |    4 |   0.0129 |        - |        - |     224 B |
| Medium_Ascii      |        38.200 ns |      0.2104 ns |      0.1968 ns |    3 |        - |        - |        - |         - |
| Medium_Mixed      |     4,213.825 ns |     43.6474 ns |     40.8278 ns |    6 |   0.1221 |        - |        - |    2216 B |
| Large_Ascii       |     4,327.400 ns |     23.7729 ns |     21.0740 ns |    6 |        - |        - |        - |         - |
| Large_Mixed       |   791,424.668 ns |  4,670.0767 ns |  4,368.3927 ns |    7 |  57.6172 |  57.6172 |  57.6172 |  220856 B |
| Large_WorstCase   | 2,275,919.826 ns | 27,753.5138 ns | 25,960.6540 ns |    8 | 105.4688 | 105.4688 | 105.4688 |  409763 B |
| Span_Medium_Mixed |     3,743.828 ns |      8.5415 ns |      7.5718 ns |    5 |   0.0038 |        - |        - |     120 B |
