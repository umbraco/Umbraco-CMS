using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class LoggerAllocationBenchmark
{
    private readonly string rawQuery = "";
    private readonly int totalItemCount;

    [Benchmark(Baseline = true)]
    public void Baseline()
    {
        for (var i = 0; i < 1000; i++)
        {
            OriginalDebugSignature(GetType(), "DeleteFromIndex with query: {Query} (found {TotalItems} results)",
                rawQuery, totalItemCount);
        }
    }

    [Benchmark]
    public void NewOverload2()
    {
        for (var i = 0; i < 1000; i++)
        {
            NewDebugSignature(GetType(), "DeleteFromIndex with query: {Query} (found {TotalItems} results)", rawQuery,
                totalItemCount);
        }
    }

    public void OriginalDebugSignature(Type reporting, string messageTemplate, params object[] propertyValues)
    {
    }

    public void NewDebugSignature<T1, T2>(Type reporting, string messageTemplate, T1 param1, T2 param2)
    {
    }

    //        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.18362
    //Intel Core i5-8265U CPU 1.60GHz(Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
    // [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0
    //  Job-JIATTD : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.8.4180.0

    //IterationCount=3  IterationTime=100.0000 ms LaunchCount = 1
    //WarmupCount=3

    //       Method |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
    //------------- |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    //     Baseline | 14.599 us | 1.0882 us | 0.0596 us |  1.00 |     10.0420 |           - |           - |             32048 B |
    // NewOverload2 |  1.775 us | 0.4056 us | 0.0222 us |  0.12 |           - |           - |           - |                   - |
}
