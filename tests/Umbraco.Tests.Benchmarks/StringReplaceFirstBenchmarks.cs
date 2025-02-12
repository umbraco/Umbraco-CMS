using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class StringReplaceFirstBenchmarks
{
    [Params(
        "Test string",
        "This is a test string that contains multiple test entries",
        "This is a string where the searched value is very far back. The system needs to go through all of this code before it reaches the test")]
    public string Text { get; set; }
    public string Search { get; set; }
    public string Replace { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Search = "test";
        Replace = "release";
    }

    [Benchmark(Baseline = true, Description = "Replace first w/ substring")]
    public string SubstringReplaceFirst()
    {
        var pos = Text.IndexOf(Search, StringComparison.InvariantCulture);

        if (pos < 0)
        {
            return Text;
        }

        return Text[..pos] + Replace + Text[(pos + Search.Length)..];
    }

    [Benchmark(Description = "Replace first w/ span")]
    public string SpanReplaceFirst()
    {
        var spanText = Text.AsSpan();
        var pos = spanText.IndexOf(Search, StringComparison.InvariantCulture);

        if (pos < 0)
        {
            return Text;
        }

        return string.Concat(spanText[..pos], Replace.AsSpan(), spanText[(pos + Search.Length)..]);
    }

    //|                       Method |                 Text |      Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Allocated |
    //|----------------------------- |--------------------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|
    //| 'Replace first w/ substring' |          Test string |  46.08 ns | 25.83 ns | 1.416 ns |  1.00 |    0.00 |      - |         - |
    //|      'Replace first w/ span' |          Test string |  38.59 ns | 19.46 ns | 1.067 ns |  0.84 |    0.05 |      - |         - |
    //|                              |                      |           |          |          |       |         |        |           |
    //| 'Replace first w/ substring' |  This(...)test[134] | 407.89 ns | 52.08 ns | 2.855 ns |  1.00 |    0.00 | 0.1833 |     584 B |
    //|      'Replace first w/ span' |  This(...)test[134] | 372.99 ns | 58.38 ns | 3.200 ns |  0.91 |    0.01 | 0.0941 |     296 B |
    //|                              |                      |           |          |          |       |         |        |           |
    //| 'Replace first w/ substring' | This(...)tries[57] | 113.16 ns | 27.95 ns | 1.532 ns |  1.00 |    0.00 | 0.0961 |     304 B |
    //|      'Replace first w/ span' | This(...)tries[57] |  76.57 ns | 17.86 ns | 0.979 ns |  0.68 |    0.01 | 0.0455 |     144 B |
}
