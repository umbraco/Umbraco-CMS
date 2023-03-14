using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class CombineGuidBenchmarks
{
    private static readonly Guid _a = Guid.NewGuid();
    private static readonly Guid _b = Guid.NewGuid();

    [Benchmark]
    public byte[] CombineUtils() => GuidUtils.Combine(_a, _b).ToByteArray();

    [Benchmark]
    public byte[] CombineLoop() => Combine(_a, _b);

    private static byte[] Combine(Guid guid1, Guid guid2)
    {
        var bytes1 = guid1.ToByteArray();
        var bytes2 = guid2.ToByteArray();
        var bytes = new byte[bytes1.Length];
        for (var i = 0; i < bytes1.Length; i++)
        {
            bytes[i] = (byte)(bytes1[i] ^ bytes2[i]);
        }

        return bytes;
    }
}

// Nov 8 2018
//BenchmarkDotNet=v0.11.2, OS=Windows 10.0.17763.55 (1809/October2018Update/Redstone5)
//Intel Core i7-6600U CPU 2.60GHz(Skylake), 1 CPU, 4 logical and 2 physical cores
//  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3190.0
//  Job-JIATTD : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3190.0

//IterationCount=3  IterationTime=100.0000 ms LaunchCount = 1
//WarmupCount=3

//       Method |     Mean |     Error |    StdDev | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
//------------- |---------:|----------:|----------:|------------:|------------:|------------:|--------------------:|
// CombineUtils | 33.34 ns |  8.086 ns | 0.4432 ns |      0.0133 |           - |           - |                28 B |
//  CombineLoop | 55.03 ns | 11.311 ns | 0.6200 ns |      0.0395 |           - |           - |                84 B |
