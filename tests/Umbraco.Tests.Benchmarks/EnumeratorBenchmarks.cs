using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
public class EnumeratorBenchmarks
{
    [Benchmark(Baseline = true)]
    public void WithArray()
    {
        foreach (var t in EnumerateOneWithArray(1))
        {
            ;
        }
    }

    [Benchmark]
    public void WithYield()
    {
        foreach (var t in EnumerateOneWithYield(1))
        {
            ;
        }
    }

    private IEnumerable<T> EnumerateOneWithArray<T>(T o) => new[] { o };

    private IEnumerable<T> EnumerateOneWithYield<T>(T o)
    {
        yield return o;
    }
}
