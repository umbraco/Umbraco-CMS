using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
[RankColumn]
[StatisticalTestColumn]
public class Utf8ToAsciiConverterBaselineBenchmarks
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

    [Benchmark]
    public string Tiny_Ascii() => OldUtf8ToAsciiConverter.ToAsciiString(TinyAscii);

    [Benchmark]
    public string Tiny_Mixed() => OldUtf8ToAsciiConverter.ToAsciiString(TinyMixed);

    [Benchmark]
    public string Small_Ascii() => OldUtf8ToAsciiConverter.ToAsciiString(SmallAscii);

    [Benchmark]
    public string Small_Mixed() => OldUtf8ToAsciiConverter.ToAsciiString(SmallMixed);

    [Benchmark]
    public string Medium_Ascii() => OldUtf8ToAsciiConverter.ToAsciiString(MediumAscii);

    [Benchmark]
    public string Medium_Mixed() => OldUtf8ToAsciiConverter.ToAsciiString(MediumMixed);

    [Benchmark]
    public string Large_Ascii() => OldUtf8ToAsciiConverter.ToAsciiString(LargeAscii);

    [Benchmark]
    public string Large_Mixed() => OldUtf8ToAsciiConverter.ToAsciiString(LargeMixed);

    [Benchmark]
    public string Large_WorstCase() => OldUtf8ToAsciiConverter.ToAsciiString(LargeWorstCase);

    [Benchmark]
    public char[] CharArray_Medium_Mixed() => OldUtf8ToAsciiConverter.ToAsciiCharArray(MediumMixed);
}
