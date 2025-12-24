```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7462)
Intel Core i9-14900K 3.20GHz, 1 CPU, 32 logical and 24 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-RELKCN : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3

IterationCount=3  IterationTime=100ms  LaunchCount=1  
WarmupCount=3  

```
| Method       | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------- |-----------:|------------:|----------:|------:|--------:|-------:|----------:|------------:|
| Baseline     | 4,186.9 ns | 2,557.63 ns | 140.19 ns |  1.00 |    0.04 | 3.3805 |   64000 B |        1.00 |
| NewOverload2 |   182.7 ns |    10.46 ns |   0.57 ns |  0.04 |    0.00 |      - |         - |        0.00 |
