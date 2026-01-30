using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[StatisticalTestColumn]
public class Utf8ToAsciiConverterBenchmarks
{
    private static readonly string TinyAscii = BenchmarkTextGenerator.GeneratePureAscii(10);
    private static readonly string TinyMixed = BenchmarkTextGenerator.GenerateMixed(10);
    private static readonly string SmallAscii = BenchmarkTextGenerator.GeneratePureAscii(100);
    private static readonly string SmallMixed = BenchmarkTextGenerator.GenerateMixed(100);
    private static readonly string MediumAscii = BenchmarkTextGenerator.GeneratePureAscii(1024);
    private static readonly string MediumMixed = BenchmarkTextGenerator.GenerateMixed(1024);
    private static readonly string LargeAscii = BenchmarkTextGenerator.GeneratePureAscii(100 * 1024);
    private static readonly string LargeMixed = BenchmarkTextGenerator.GenerateMixed(100 * 1024);
    private static readonly string LargeWorstCase = BenchmarkTextGenerator.GenerateWorstCase(100 * 1024);

    private IUtf8ToAsciiConverter _converter = null!;

    [GlobalSetup]
    public void Setup()
    {
        var hostEnv = new HostingEnvironment { ContentRootPath = AppContext.BaseDirectory };
        var loader = new CharacterMappingLoader(hostEnv, NullLogger<CharacterMappingLoader>.Instance);
        _converter = new Utf8ToAsciiConverter(loader);
    }

    [Benchmark]
    public string Tiny_Ascii() => _converter.Convert(TinyAscii);

    [Benchmark]
    public string Tiny_Mixed() => _converter.Convert(TinyMixed);

    [Benchmark]
    public string Small_Ascii() => _converter.Convert(SmallAscii);

    [Benchmark]
    public string Small_Mixed() => _converter.Convert(SmallMixed);

    [Benchmark]
    public string Medium_Ascii() => _converter.Convert(MediumAscii);

    [Benchmark]
    public string Medium_Mixed() => _converter.Convert(MediumMixed);

    [Benchmark]
    public string Large_Ascii() => _converter.Convert(LargeAscii);

    [Benchmark]
    public string Large_Mixed() => _converter.Convert(LargeMixed);

    [Benchmark]
    public string Large_WorstCase() => _converter.Convert(LargeWorstCase);

    [Benchmark]
    public int Span_Medium_Mixed()
    {
        Span<char> buffer = stackalloc char[4096];
        return _converter.Convert(MediumMixed.AsSpan(), buffer);
    }
}
