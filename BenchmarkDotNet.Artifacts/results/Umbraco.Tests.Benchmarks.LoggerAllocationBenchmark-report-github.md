```

BenchmarkDotNet v0.15.6, Linux Ubuntu 25.10 (Questing Quokka)
Intel Xeon CPU 2.80GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
  Job-RELKCN : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4

IterationCount=3  IterationTime=100ms  LaunchCount=1  
WarmupCount=3  

```
| Method   | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|--------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
| Baseline | 19.08 μs | 3.570 μs | 0.196 μs |  1.00 | 3.5554 |   62.5 KB |        1.00 |
