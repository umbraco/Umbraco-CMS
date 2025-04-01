using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class StringStartsWithBenchmarks
{

    private string _domainName = "domain1.com";

    [Benchmark(Baseline = true)]
    public bool Original()
    {
        return _domainName.StartsWith("/");
    }

    [Benchmark()]
    public bool Ordinal()
    {
        return _domainName.StartsWith("/",StringComparison.Ordinal);
    }

    [Benchmark()]
    public bool Invariant()
    {
        return _domainName.StartsWith("/", StringComparison.InvariantCulture);
    }

    [Benchmark()]
    public bool FirstChar()
    {
        return _domainName.Length > 0 && _domainName[0] == '/';
    }

    [Benchmark()]
    public bool Span()
    {
        return _domainName.AsSpan().StartsWith("/".AsSpan(),StringComparison.Ordinal);
    }

    /*
       | Method    | Mean        | Error      | StdDev    | Allocated |
       |---------- |------------:|-----------:|----------:|----------:|
       | Original  | 255.2239 ns | 10.9432 ns | 0.5998 ns |         - |
       | Ordinal   |   0.1784 ns |  0.3070 ns | 0.0168 ns |         - |
       | Invariant |   4.1270 ns |  0.4990 ns | 0.0274 ns |         - |
       | FirstChar |   0.0127 ns |  0.0098 ns | 0.0005 ns |         - |
       | Span      |   0.8000 ns |  0.4526 ns | 0.0248 ns |         - |
     */

}
