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

    [Benchmark(Baseline = true)]
    public void ToUrlSegment()
    {
        _shortStringHelper.CleanStringForUrlSegment(_input);
    }

    /*[Benchmark(Baseline = true)]
    public string OldAsciString()
    {
        return OldUtf8ToAsciiConverter.ToAsciiString(_input);
    }


    [Benchmark]
    public string NewAsciString()
    {
        return Utf8ToAsciiConverter.ToAsciiString(_input);
    }*/

    #region SurrogatePairs

    /*[Benchmark(Baseline = true)]
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
    }*/

    #endregion

    //|       Method                       |     Mean |    Error |  StdDev | Ratio |  Gen 0 | Allocated |
    //|-----------------------------------:|---------:|---------:|--------:|------:|-------:|----------:|
    //| ToUrlSegment                       | 464.2 ns | 34.88 ns | 1.91 ns |  1.00 | 0.1627 |     512 B |
    //| ToUrlSegment (With below changes)  | 455.7 ns | 26.83 ns | 1.47 ns |  1.00 | 0.1182 |     384 B |
    //| ToUrlSegment(CleanCodeString change| 420.6 ns | 64.06 ns | 3.51 ns |  1.00 | 0.0856 |     280 B |

    //|                  Method |     Mean |     Error |   StdDev | Ratio |  Gen 0 | Allocated |
    //|------------------------ |---------:|----------:|---------:|------:|-------:|----------:|
    //|    RemoveSurrogatePairs | 70.75 ns | 15.307 ns | 0.839 ns |  1.00 | 0.0610 |     192 B |
    //| RemoveNewSurrogatePairs | 58.44 ns |  7.297 ns | 0.400 ns |  0.83 | 0.0198 |      64 B |

    //|        Method |     Mean |    Error |  StdDev | Ratio |  Gen 0 | Allocated |
    //|-------------- |---------:|---------:|--------:|------:|-------:|----------:|
    //| OldAsciString | 181.4 ns | 11.50 ns | 0.63 ns |  1.00 | 0.0851 |     272 B |
    //| NewAsciString | 180.7 ns |  5.35 ns | 0.29 ns |  1.00 | 0.0450 |     64 B |
}
