```

BenchmarkDotNet v0.15.6, Linux Ubuntu 25.10 (Questing Quokka)
Intel Xeon CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100-rc.2.25502.107
  [Host]   : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4
  ShortRun : .NET 10.0.0 (10.0.0-rc.2.25502.107, 10.0.25.50307), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                                                | Mean           | Error         | StdDev      | Median         | Gen0   | Allocated |
|------------------------------------------------------ |---------------:|--------------:|------------:|---------------:|-------:|----------:|
| Linq                                                  | 51,195.5380 ns | 1,617.4345 ns |  88.6570 ns | 51,163.7063 ns | 3.4180 |   59712 B |
| SplitToHeapStrings                                    | 37,354.8894 ns | 9,999.4406 ns | 548.1031 ns | 37,333.8901 ns | 2.5635 |   44592 B |
| SplitToStackSpansWithoutEmptyCheckReversingListAsSpan | 25,784.9531 ns | 1,949.3238 ns | 106.8490 ns | 25,818.4337 ns | 0.9766 |   17128 B |
| SplitToStackSpansWithoutEmptyCheck                    | 26,441.8317 ns | 4,054.8077 ns | 222.2577 ns | 26,557.4375 ns | 0.9766 |   17128 B |
| SplitToStackSpansWithEmptyCheck                       | 25,821.9195 ns | 4,840.3751 ns | 265.3173 ns | 25,718.1962 ns | 0.9766 |   17128 B |
| StripWhitespace_Benchmark                             |    269.2084 ns |    46.5960 ns |   2.5541 ns |    267.8466 ns | 0.0033 |      64 B |
| GetFileExtension_Benchmark                            |    308.9820 ns |   100.8086 ns |   5.5257 ns |    309.7014 ns | 0.0319 |     552 B |
| StripHtml_Benchmark                                   |    719.6788 ns |   182.4947 ns |  10.0031 ns |    718.6075 ns | 0.0019 |      48 B |
| IsLowerCase_Benchmark                                 |      0.0194 ns |     0.2102 ns |   0.0115 ns |      0.0218 ns |      - |         - |
| IsUpperCase_Benchmark                                 |      0.0078 ns |     0.2461 ns |   0.0135 ns |      0.0000 ns |      - |         - |
| ReplaceNonAlphanumericChars_String_Benchmark          |     84.6292 ns |    48.9647 ns |   2.6839 ns |     84.3141 ns | 0.0097 |     168 B |
