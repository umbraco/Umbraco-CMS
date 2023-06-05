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


    //This is the fastest, however we the check will be less good than before as it'll return true on things like this: [test or {test]
    [Benchmark]
    public bool CharRangeIndexDetectJsonBad()
    {
        var input = Input;
        return input[0] is '{' or '[' || input[^1] is '}' or ']';
    }

    [Benchmark]
    public bool CharDetectJsonTwoLookups()
    {
        var input = Input.Trim();
        var firstChar = input[0];
        var lastChar = input[^1];
        return (firstChar is '[' && lastChar is ']') || (firstChar is '{' && lastChar is '}');
    }


//|                      Method |        Mean |     Error |    StdDev | Ratio | Allocated |
//|---------------------------- |------------:|----------:|----------:|------:|----------:|
//|            StringDetectJson | 103.7203 ns | 1.5370 ns | 0.0842 ns | 1.000 |         - |
//|              CharDetectJson |   8.8119 ns | 1.0330 ns | 0.0566 ns | 0.085 |         - |
//|    CharRangeIndexDetectJson |   7.8054 ns | 1.2396 ns | 0.0679 ns | 0.075 |         - |
//| CharRangeIndexDetectJsonBad |   0.4597 ns | 0.1882 ns | 0.0103 ns | 0.004 |         - |
//|    CharDetectJsonTwoLookups |   7.8292 ns | 1.7397 ns | 0.0954 ns | 0.075 |         - |
}
