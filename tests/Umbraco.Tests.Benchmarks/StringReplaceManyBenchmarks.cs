using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class StringReplaceManyBenchmarks
{
    static StringReplaceManyBenchmarks()
    {
        // pick what you want to benchmark

        // short
        Text = "1,2.3:4&5#6";

        // long
        //Text = "Sed ut perspiciatis unde omnis iste natus &error sit voluptatem accusantium doloremque l:audantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et &quasi architecto beatae vitae ::dicta sunt explicabo. Nemo enim ipsam volupta:tem quia voluptas sit aspernatur aut o&dit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciun&t. Neque porro quisquam est, qui dolorem: ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut e:nim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi co&&nsequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse: quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?";

        // short
        Replacements = new Dictionary<string, string>
        {
            {",", "*"},
            {".", "*"},
            {":", "*"},
            {"&", "*"},
            {"#", "*"}
        };

        // long
        //Replacements = new Dictionary<string, string>();
        //for (var i = 2; i < 100; i++)
        //    Replacements[Convert.ToChar(i).ToString()] = "*";
    }

    // this is what v7 originally did
    [Benchmark(Description = "String.ReplaceMany w/chars - Aggregate", Baseline = true)]
    public string ReplaceManyAggregate()
    {
        var result = Text;
        return ReplacedChars.Aggregate(result, (current, c) => current.Replace(c, ReplacementChar));
    }

    [Benchmark(Description = "String.ReplaceMany w/chars - For Loop")]
    public string ReplaceManyForLoop()
    {
        var result = Text;

        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < ReplacedChars.Length; i++)
        {
            result = result.Replace(ReplacedChars[i], ReplacementChar);
        }

        return result;
    }

    // this is what v7 originally did
    [Benchmark(Description = "String.ReplaceMany w/dictionary - Aggregate")]
    public string ReplaceManyDictionaryAggregate() =>
        Replacements.Aggregate(Text, (current, kvp) => current.Replace(kvp.Key, kvp.Value));

    [Benchmark(Description = "String.ReplaceMany w/dictionary - For Each")]
    public string ReplaceManyDictionaryForEach()
    {
        var result = Text;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var item in Replacements)
        {
            result = result.Replace(item.Key, item.Value);
        }

        return result;
    }
    /*

    short text, short replacement:

                                           Method |     Mean |     Error |    StdDev | Scaled | ScaledSD |  Gen 0 | Allocated |
    --------------------------------------------- |---------:|----------:|----------:|-------:|---------:|-------:|----------:|
    'String.ReplaceMany w/chars - Aggregate'      | 236.0 ns |  40.92 ns |  2.312 ns |   1.00 |     0.00 | 0.0461 |     200 B |
    'String.ReplaceMany w/chars - For Loop'       | 166.7 ns |  70.51 ns |  3.984 ns |   0.71 |     0.01 | 0.0420 |     180 B |
    'String.ReplaceMany w/dictionary - Aggregate' | 606.5 ns | 342.94 ns | 19.377 ns |   2.57 |     0.07 | 0.0473 |     212 B |
    'String.ReplaceMany w/dictionary - For Each'  | 571.8 ns | 232.33 ns | 13.127 ns |   2.42 |     0.05 | 0.0458 |     212 B |

    long text, short replacement:

                                           Method |      Mean |     Error |    StdDev | Scaled | ScaledSD |  Gen 0 | Allocated |
    --------------------------------------------- |----------:|----------:|----------:|-------:|---------:|-------:|----------:|
    'String.ReplaceMany w/chars - Aggregate'      |  5.771 us |  9.963 us | 0.5630 us |   1.00 |     0.00 | 1.6798 |   6.94 KB |
    'String.ReplaceMany w/chars - For Loop'       |  4.962 us |  2.121 us | 0.1199 us |   0.87 |     0.08 | 1.6840 |   6.92 KB |
    'String.ReplaceMany w/dictionary - Aggregate' | 14.514 us |  8.189 us | 0.4627 us |   2.53 |     0.22 | 1.6447 |   6.96 KB |
    'String.ReplaceMany w/dictionary - For Each'  | 15.445 us | 24.745 us | 1.3981 us |   2.69 |     0.30 | 1.5696 |   6.96 KB |

    short text, long replacements dictionary:

                                           Method |       Mean |      Error |    StdDev | Scaled | ScaledSD |  Gen 0 | Allocated |
    --------------------------------------------- |-----------:|-----------:|----------:|-------:|---------:|-------:|----------:|
    'String.ReplaceMany w/chars - Aggregate'      |   257.0 ns |   200.0 ns |  11.30 ns |   1.00 |     0.00 | 0.0452 |     200 B |
    'String.ReplaceMany w/chars - For Loop'       |   182.4 ns |   221.0 ns |  12.49 ns |   0.71 |     0.05 | 0.0425 |     180 B |
    'String.ReplaceMany w/dictionary - Aggregate' | 7,273.8 ns | 2,747.1 ns | 155.22 ns |  28.34 |     1.12 | 0.0714 |     464 B |
    'String.ReplaceMany w/dictionary - For Each'  | 6,981.0 ns | 5,500.7 ns | 310.80 ns |  27.20 |     1.38 | 0.0775 |     464 B |

    long text, long replacements dictionary:

                                           Method |       Mean |      Error |     StdDev | Scaled | ScaledSD |  Gen 0 | Allocated |
    --------------------------------------------- |-----------:|-----------:|-----------:|-------:|---------:|-------:|----------:|
    'String.ReplaceMany w/chars - Aggregate'      |   4.868 us |   3.420 us |  0.1932 us |   1.00 |     0.00 | 1.6816 |   6.94 KB |
    'String.ReplaceMany w/chars - For Loop'       |   4.958 us |   2.633 us |  0.1487 us |   1.02 |     0.04 | 1.6791 |   6.92 KB |
    'String.ReplaceMany w/dictionary - Aggregate' | 181.309 us | 210.177 us | 11.8754 us |  37.29 |     2.32 | 5.3571 |  24.28 KB |
    'String.ReplaceMany w/dictionary - For Each'  | 174.567 us | 113.733 us |  6.4262 us |  35.90 |     1.57 | 5.8594 |  24.28 KB |

    */

    // don't use constants
    // ReSharper disable ConvertToConstant.Local

    // input text for ReplaceMany
    private static readonly string Text;

    // replaced chars for ReplaceMany with chars
    private static readonly char[] ReplacedChars = { ',', '.', ':', '&', '#' };

    // replacement char for ReplaceMany with chars
    private static readonly char ReplacementChar = '*';

    // replacements for ReplaceMany with dictionary
    private static readonly IDictionary<string, string> Replacements;

    // ReSharper restore ConvertToConstant.Local
}
