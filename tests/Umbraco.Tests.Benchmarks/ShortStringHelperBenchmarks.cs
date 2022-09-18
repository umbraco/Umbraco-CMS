using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core.Strings;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class ShortStringHelperBenchmarks
{
    private DefaultShortStringHelper _shortStringHelper;

    private string _input;

    [GlobalSetup]
    public void Setup()
    {
        _shortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig());
        _input = "This is a 🎈 balloon";
    }

    /*[Benchmark(Baseline = true)]
    public void ToUrlSegment()
    {
        _shortStringHelper.CleanStringForUrlSegment(_input);
    }*/

    [Benchmark(Baseline = true)]
    public string RemoveSurrogatePairs()
    {
        var input = _input.ToCharArray();
        var output = new char[input.Length];
        var opos = 0;

        for (var ipos = 0; ipos < input.Length; ipos++)
        {
            var c = input[ipos];
            if (char.IsSurrogate(c)) // ignore high surrogate
            {
                ipos++; // and skip low surrogate
                output[opos++] = '?';
            }
            else
            {
                output[opos++] = c;
            }
        }

        return new string(output, 0, opos);
    }

    [Benchmark]
    public string RemoveNewSurrogatePairs()
    {
        var input = _input.AsSpan();
        Span<char> output = input.Length <= 1024 ? stackalloc char[input.Length] : new char[input.Length];
        var opos = 0;

        for (var ipos = 0; ipos < input.Length; ipos++)
        {
            var c = input[ipos];
            if (char.IsSurrogate(c)) // ignore high surrogate
            {
                ipos++; // and skip low surrogate
                output[opos++] = '?';
            }
            else
            {
                output[opos++] = c;
            }
        }

        return new string(output);
    }

    //|       Method |     Mean |    Error |  StdDev | Ratio |  Gen 0 | Allocated |
    //|------------- |---------:|---------:|--------:|------:|-------:|----------:|
    //| ToUrlSegment | 464.2 ns | 34.88 ns | 1.91 ns |  1.00 | 0.1627 |     512 B |

    //|                  Method |     Mean |     Error |   StdDev | Ratio |  Gen 0 | Allocated |
    //|------------------------ |---------:|----------:|---------:|------:|-------:|----------:|
    //|    RemoveSurrogatePairs | 70.75 ns | 15.307 ns | 0.839 ns |  1.00 | 0.0610 |     192 B |
    //| RemoveNewSurrogatePairs | 58.44 ns |  7.297 ns | 0.400 ns |  0.83 | 0.0198 |      64 B |
}
