using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunConfig]
public class HexStringBenchmarks
{
    private byte[] _buffer;

    [Params(8, 16, 32, 64, 128, 256)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new byte[Count];
        var random = new Random();
        random.NextBytes(_buffer);
    }

    [Benchmark(Baseline = true)]
    public string ToHexStringBuilder()
    {
        var sb = new StringBuilder(_buffer.Length * 2);
        for (var i = 0; i < _buffer.Length; i++)
        {
            sb.Append(_buffer[i].ToString("X2"));
        }

        return sb.ToString();
    }

    [Benchmark]
    public string ToHexStringEncoder() => HexEncoder.Encode(_buffer);
}

// Nov 8 2018
//BenchmarkDotNet=v0.11.2, OS=Windows 10.0.17763.55 (1809/October2018Update/Redstone5)
//Intel Core i7-6600U CPU 2.60GHz(Skylake), 1 CPU, 4 logical and 2 physical cores
// [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3190.0
//  Job-JIATTD : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3190.0

//IterationCount=3  IterationTime=100.0000 ms LaunchCount = 1
//WarmupCount=3

//             Method | Count |         Mean |        Error |     StdDev | Ratio |
//------------------- |------ |-------------:|-------------:|-----------:|------:|
// ToHexStringBuilder |     8 |    786.49 ns |    319.92 ns |  17.536 ns |  1.00 |
// ToHexStringEncoder |     8 |     64.19 ns |     30.21 ns |   1.656 ns |  0.08 |
//                    |       |              |              |            |       |
// ToHexStringBuilder |    16 |  1,442.43 ns |    503.00 ns |  27.571 ns |  1.00 |
// ToHexStringEncoder |    16 |    133.46 ns |    177.55 ns |   9.732 ns |  0.09 |
//                    |       |              |              |            |       |
// ToHexStringBuilder |    32 |  2,869.23 ns |    924.35 ns |  50.667 ns |  1.00 |
// ToHexStringEncoder |    32 |    181.03 ns |     96.64 ns |   5.297 ns |  0.06 |
//                    |       |              |              |            |       |
// ToHexStringBuilder |    64 |  5,775.33 ns |  2,825.42 ns | 154.871 ns |  1.00 |
// ToHexStringEncoder |    64 |    331.16 ns |    125.63 ns |   6.886 ns |  0.06 |
//                    |       |              |              |            |       |
// ToHexStringBuilder |   128 | 11,662.35 ns |  4,908.03 ns | 269.026 ns |  1.00 |
// ToHexStringEncoder |   128 |    633.78 ns |     57.56 ns |   3.155 ns |  0.05 |
//                    |       |              |              |            |       |
// ToHexStringBuilder |   256 | 22,960.11 ns | 14,111.47 ns | 773.497 ns |  1.00 |
// ToHexStringEncoder |   256 |  1,224.76 ns |    547.27 ns |  29.998 ns |  0.05 |
