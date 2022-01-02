``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1288 (21H1/May2021Update)
Intel Core i7-4710HQ CPU 2.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.208
  [Host]     : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT
  DefaultJob : .NET 5.0.11 (5.0.1121.47308), X64 RyuJIT


```
|         Method |     Mean |   Error |  StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
|--------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|
|     EnsureUrls | 124.7 ns | 2.19 ns | 2.52 ns |  1.00 |    0.00 | 0.0381 |     120 B |
| EnsureUrlsTest | 126.4 ns | 1.73 ns | 1.53 ns |  1.02 |    0.03 | 0.0381 |     120 B |
