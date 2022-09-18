using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class GetSafeQueryBenchmarks
{
    private Uri _input;

    [GlobalSetup]
    public void Setup()
    {
        _input = new Uri("this-is-a-test?query=test&test=2", UriKind.Relative);
    }

    [Benchmark(Baseline = true)]
    public string OrigGetSafeQuery()
    {
        // cannot get .Query on relative uri (InvalidOperation)
        var s = _input.OriginalString;
        var posq = s.IndexOf("?", StringComparison.Ordinal);
        var posf = s.IndexOf("#", StringComparison.Ordinal);
        var query = posq < 0 ? null : (posf < 0 ? s.Substring(posq) : s.Substring(posq, posf - posq));

        return query;
    }

    [Benchmark]
    public string GetSafeQueryWithChars()
    {
        // cannot get .Query on relative uri (InvalidOperation)
        var s = _input.OriginalString;
        var posq = s.IndexOf('?', StringComparison.Ordinal);
        var posf = s.IndexOf('#', StringComparison.Ordinal);
        var query = posq < 0 ? null : (posf < 0 ? s[posq..] : s[posq..posf]);

        return query;
    }
}
