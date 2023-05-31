using System;
using BenchmarkDotNet.Attributes;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks
{
    [QuickRunWithMemoryDiagnoserConfig]
    public class StringFileExtensionBenchmark
    {
        [Arguments("smallText.txt")]
        [Arguments("aVeryLongTextThatContainsALotOfCharacters.txt")]
        [Arguments("NotEvenAFile")]
        [Benchmark(Baseline = true)]
        public string StringStrip(string fileName)
        {
            // filenames cannot contain line breaks
            if (fileName.Contains(Environment.NewLine) || fileName.Contains("\r") || fileName.Contains("\n"))
            {
                return fileName;
            }

            var lastIndex = fileName.LastIndexOf('.');
            if (lastIndex > 0)
            {
                var ext = fileName.Substring(lastIndex);

                // file extensions cannot contain whitespace
                if (ext.Contains(" "))
                {
                    return fileName;
                }

                return string.Format("{0}", fileName.Substring(0, fileName.IndexOf(ext, StringComparison.Ordinal)));
            }

            return fileName;
        }

        [Arguments("smallText.txt")]
        [Arguments("aVeryLongTextThatContainsALotOfCharacters.txt")]
        [Arguments("NotEvenAFile")]
        [Benchmark]
        public string SpanStrip(string fileName)
        {
            // filenames cannot contain line breaks
            if (fileName.Contains(Environment.NewLine) || fileName.Contains("\r") || fileName.Contains("\n"))
            {
                return fileName;
            }

            var spanFileName = fileName.AsSpan();
            var lastIndex = spanFileName.LastIndexOf('.');
            if (lastIndex > 0)
            {
                var ext = spanFileName[lastIndex..];

                // file extensions cannot contain whitespace
                if (ext.Contains(' '))
                {
                    return fileName;
                }

                return new string(spanFileName[..lastIndex]);
            }

            return fileName;
        }
    }

    //|      Method |             fileName |      Mean |     Error |   StdDev | Ratio |  Gen 0 | Allocated |
    //|------------ |--------------------- |----------:|----------:|---------:|------:|-------:|----------:|
    //| StringStrip |         NotEvenAFile |  45.08 ns |  1.277 ns | 0.070 ns |  1.00 |      - |         - |
    //|   SpanStrip |         NotEvenAFile |  45.13 ns |  6.131 ns | 0.336 ns |  1.00 |      - |         - |
    //|             |                      |           |           |          |       |        |           |
    //| StringStrip | aVery(...)s.txt[45] | 234.10 ns | 28.303 ns | 1.551 ns |  1.00 | 0.0751 |     240 B |
    //|   SpanStrip | aVery(...)s.txt[45] |  98.37 ns | 14.839 ns | 0.813 ns |  0.42 | 0.0331 |     104 B |
    //|             |                      |           |           |          |       |        |           |
    //| StringStrip |        smallText.txt | 187.79 ns | 35.672 ns | 1.955 ns |  1.00 | 0.0348 |     112 B |
    //|   SpanStrip |        smallText.txt |  62.46 ns | 13.795 ns | 0.756 ns |  0.33 | 0.0124 |      40 B |
}
