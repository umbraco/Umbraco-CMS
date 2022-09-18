using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class DetectJsonBenchmarks
{
    private string Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Input = "[{test: true},{test:false}]";
    }

    [Benchmark(Baseline = true)]
    public bool StringDetectJson()
    {
        var input = Input.Trim();
        return (input.StartsWith("{") && input.EndsWith("}"))
               || (input.StartsWith("[") && input.EndsWith("]"));
    }

    [Benchmark]
    public bool CharDetectJson()
    {
        var input = Input.Trim();
        return (input.StartsWith('{') && input.EndsWith('}'))
               || (input.StartsWith('[') && input.EndsWith(']'));
    }

    [Benchmark]
    public bool CharRangeIndexDetectJson()
    {
        var input = Input.Trim();
        return (input[0] is '[' && input[^1] is ']') || (input[0] is '{' && input[^1] is '}');
    }

    [Benchmark]
    public bool CharRangeIndexDetectJsonBad()
    {
        var input = Input;
        return input[0] is '{' or '[' || input[^1] is '}' or ']';
    }


    //|           Method |      Mean |      Error |     StdDev | Ratio | RatioSD | Allocated |
    //|----------------- |----------:|-----------:|-----------:|------:|--------:|----------:|
    //| StringDetectJson | 96.580 ns | 285.565 ns | 15.6528 ns |  1.00 |    0.00 |         - |
    //|   CharDetectJson |  8.846 ns |   1.220 ns |  0.0669 ns |  0.09 |    0.02 |         - |
}
