using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class StringIndexOfBenchmarks
{
    private string _domainName = "https://www.lorem-ipsum.com";

    [Benchmark()]
    public bool IndexOf_Original()
    {
        return _domainName.IndexOf("://") > 0;
    }

    [Benchmark()]
    public bool IndexOf_Ordinal()
    {
        return _domainName.IndexOf("://", StringComparison.Ordinal) > -1;
    }

    [Benchmark()]
    public bool IndexOf_Invariant()
    {
        return _domainName.IndexOf("://", StringComparison.InvariantCulture) > -1;
    }

    [Benchmark()]
    public bool IndexOf_Span()
    {
        return _domainName.AsSpan().IndexOf("://", StringComparison.InvariantCulture) > -1;
    }

    [Benchmark()]
    public bool Contains()
    {
        return _domainName.Contains("://");
    }

    [Benchmark()]
    public bool Contains_Ordinal()
    {
        return _domainName.Contains("://", StringComparison.Ordinal);
    }

    [Benchmark()]
    public bool Contains_Invariant()
    {
        return _domainName.Contains("://", StringComparison.InvariantCulture);
    }

    [Benchmark()]
    public bool Contains_Span_Ordinal()
    {
        return _domainName.AsSpan().Contains("://", StringComparison.Ordinal);
    }

    [Benchmark()]
    public bool Contains_Span_Invariant()
    {
        return _domainName.AsSpan().Contains("://", StringComparison.InvariantCulture);
    }

    [Benchmark()]
    public bool Span_Index_Of()
    {
        var uri = "https://www.lorem-ipsum.com".AsSpan();
        return uri.IndexOf("#") > -1;
    }

    [Benchmark()]
    public bool Span_Index_Of_Ordinal()
    {
        var uri = "https://www.lorem-ipsum.com".AsSpan();
        return uri.IndexOf("#".AsSpan(), StringComparison.Ordinal) > -1;
    }

    /*
       | Method                  | Mean       | Error      | StdDev    | Allocated |
       |------------------------ |-----------:|-----------:|----------:|----------:|
       | IndexOf_Original        | 916.918 ns | 73.7556 ns | 4.0428 ns |         - |
       | IndexOf_Ordinal         |   4.083 ns |  1.5083 ns | 0.0827 ns |         - |
       | IndexOf_Invariant       |  12.941 ns |  3.7574 ns | 0.2060 ns |         - |
       | IndexOf_Span            |  13.076 ns |  3.0666 ns | 0.1681 ns |         - |
       | Contains                |   2.828 ns |  0.3648 ns | 0.0200 ns |         - |
       | Contains_Ordinal        |   4.368 ns |  0.9882 ns | 0.0542 ns |         - |
       | Contains_Invariant      |  12.986 ns |  2.3526 ns | 0.1290 ns |         - |
       | Contains_Span_Ordinal   |   2.924 ns |  0.1593 ns | 0.0087 ns |         - |
       | Contains_Span_Invariant |  12.502 ns |  1.4153 ns | 0.0776 ns |         - |
       | Span_Index_Of           |   1.741 ns |  0.9093 ns | 0.0498 ns |         - |
       | Span_Index_Of_Ordinal   |   1.809 ns |  0.3703 ns | 0.0203 ns |         - |
     */
}
