using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunConfig]
[MemoryDiagnoser]
public class JsonSerializerSettingsBenchmarks
{
    [Benchmark]
    public void SerializerSettingsInstantiation()
    {
        var instances = 1000;
        for (var i = 0; i < instances; i++)
        {
            new JsonSerializerSettings();
        }
    }

    [Benchmark(Baseline = true)]
    public void SerializerSettingsSingleInstantiation() => new JsonSerializerSettings();

    //        // * Summary *

    //        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.18362
    //Intel Core i5-8265U CPU 1.60GHz(Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    // [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4250.0
    //  Job-JIATTD : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4250.0

    //IterationCount=3  IterationTime=100.0000 ms LaunchCount = 1
    //WarmupCount=3

    //                                Method |         Mean |        Error |      StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
    //-------------------------------------- |-------------:|-------------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
    //       SerializerSettingsInstantiation | 29,120.48 ns | 5,532.424 ns | 303.2508 ns | 997.84 |   23.66 |     73.8122 |           - |           - |            232346 B |
    // SerializerSettingsSingleInstantiation |     29.19 ns |     8.089 ns |   0.4434 ns |   1.00 |    0.00 |      0.0738 |           - |           - |               232 B |

    //// * Warnings *
    //MinIterationTime
    //  JsonSerializerSettingsBenchmarks.SerializerSettingsSingleInstantiation: IterationCount= 3, IterationTime= 100.0000 ms, LaunchCount= 1, WarmupCount= 3->MinIterationTime = 96.2493 ms which is very small. It's recommended to increase it.

    //// * Legends *
    //  Mean                : Arithmetic mean of all measurements
    //  Error               : Half of 99.9% confidence interval
    //  StdDev              : Standard deviation of all measurements
    //  Ratio               : Mean of the ratio distribution ([Current]/[Baseline])
    //  RatioSD             : Standard deviation of the ratio distribution([Current]/[Baseline])
    //  Gen 0/1k Op         : GC Generation 0 collects per 1k Operations
    //  Gen 1/1k Op         : GC Generation 1 collects per 1k Operations
    //  Gen 2/1k Op         : GC Generation 2 collects per 1k Operations
    //  Allocated Memory/Op : Allocated memory per single operation(managed only, inclusive, 1KB = 1024B)
    //  1 ns                : 1 Nanosecond(0.000000001 sec)

    //// * Diagnostic Output - MemoryDiagnoser *


    //        // ***** BenchmarkRunner: End *****
    //        Run time: 00:00:04 (4.88 sec), executed benchmarks: 2
}
