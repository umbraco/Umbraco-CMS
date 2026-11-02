using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Umbraco.Extensions;
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

    [Benchmark(Description = "String.ReplaceMany w/chars - String Create")]
    public string ReplaceManyStringCreate() => Text.ReplaceMany(ReplacedChars, ReplacementChar);

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

    | Method                                        | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    |---------------------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
    | 'String.ReplaceMany w/chars - Aggregate'      | 68.65 ns | 46.32 ns | 2.539 ns |  1.00 |    0.04 | 0.0185 |     240 B |        1.00 |
    | 'String.ReplaceMany w/chars - For Loop'       | 66.85 ns | 10.66 ns | 0.584 ns |  0.97 |    0.03 | 0.0190 |     240 B |        1.00 |
    | 'String.ReplaceMany w/chars - String Create'  | 39.22 ns | 13.80 ns | 0.757 ns |  0.57 |    0.02 | 0.0037 |      48 B |        0.20 |
    | 'String.ReplaceMany w/dictionary - Aggregate' | 75.53 ns | 26.43 ns | 1.449 ns |  1.10 |    0.04 | 0.0189 |     240 B |        1.00 |
    | 'String.ReplaceMany w/dictionary - For Each'  | 73.10 ns | 17.46 ns | 0.957 ns |  1.07 |    0.04 | 0.0186 |     240 B |        1.00 |

    long text, short replacement:

    | Method                                        | Mean     | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    |---------------------------------------------- |---------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
    | 'String.ReplaceMany w/chars - Aggregate'      | 426.0 ns | 856.13 ns | 46.93 ns |  1.01 |    0.14 | 0.5656 |   6.97 KB |        1.00 |
    | 'String.ReplaceMany w/chars - For Loop'       | 415.0 ns | 230.56 ns | 12.64 ns |  0.98 |    0.10 | 0.5658 |   6.97 KB |        1.00 |
    | 'String.ReplaceMany w/chars - String Create'  | 389.8 ns | 177.93 ns |  9.75 ns |  0.92 |    0.09 | 0.1401 |   1.74 KB |        0.25 |
    | 'String.ReplaceMany w/dictionary - Aggregate' | 414.8 ns | 191.44 ns | 10.49 ns |  0.98 |    0.10 | 0.5668 |   6.97 KB |        1.00 |
    | 'String.ReplaceMany w/dictionary - For Each'  | 419.2 ns |  87.69 ns |  4.81 ns |  0.99 |    0.10 | 0.5653 |   6.97 KB |        1.00 |

    short text, long replacements dictionary:

    | Method                                        | Mean      | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    |---------------------------------------------- |----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
    | 'String.ReplaceMany w/chars - Aggregate'      |  71.42 ns |  13.767 ns |  0.755 ns |  1.00 |    0.01 | 0.0185 |     240 B |        1.00 |
    | 'String.ReplaceMany w/chars - For Loop'       |  69.19 ns |  36.993 ns |  2.028 ns |  0.97 |    0.03 | 0.0190 |     240 B |        1.00 |
    | 'String.ReplaceMany w/chars - String Create'  |  39.95 ns |   4.868 ns |  0.267 ns |  0.56 |    0.01 | 0.0035 |      48 B |        0.20 |
    | 'String.ReplaceMany w/dictionary - Aggregate' | 639.32 ns | 184.521 ns | 10.114 ns |  8.95 |    0.15 | 0.0389 |     528 B |        2.20 |
    | 'String.ReplaceMany w/dictionary - For Each'  | 662.39 ns | 369.149 ns | 20.234 ns |  9.28 |    0.26 | 0.0399 |     528 B |        2.20 |

    long text, long replacements dictionary:

    | Method                                        | Mean       | Error     | StdDev   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    |---------------------------------------------- |-----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
    | 'String.ReplaceMany w/chars - Aggregate'      |   415.0 ns | 402.10 ns | 22.04 ns |  1.00 |    0.06 | 0.5657 |   6.97 KB |        1.00 |
    | 'String.ReplaceMany w/chars - For Loop'       |   383.4 ns | 148.00 ns |  8.11 ns |  0.93 |    0.05 | 0.5664 |   6.97 KB |        1.00 |
    | 'String.ReplaceMany w/chars - String Create'  |   363.5 ns |  57.10 ns |  3.13 ns |  0.88 |    0.04 | 0.1411 |   1.74 KB |        0.25 |
    | 'String.ReplaceMany w/dictionary - Aggregate' | 2,981.0 ns | 893.78 ns | 48.99 ns |  7.20 |    0.34 | 1.8340 |  22.65 KB |        3.25 |
    | 'String.ReplaceMany w/dictionary - For Each'  | 2,947.4 ns | 552.32 ns | 30.27 ns |  7.12 |    0.33 | 1.8223 |  22.65 KB |        3.25 |

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
