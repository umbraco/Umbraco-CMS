using System.Globalization;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
public class StringExtensionsBenchmarks
{
    private static readonly Random _seededRandom = new(60);
    private const int Size = 100;
    private static readonly string[] _stringsWithCommaSeparatedNumbers = new string[Size];

    static StringExtensionsBenchmarks()
    {
        for (var i = 0; i < Size; i++)
        {
            int countOfNumbers = _seededRandom.Next(2, 10); // guess on path lengths in normal use
            int[] randomIds = new int[countOfNumbers];
            for (var i1 = 0; i1 < countOfNumbers; i1++)
            {
                randomIds[i1] = _seededRandom.Next(-1, int.MaxValue);
            }

            _stringsWithCommaSeparatedNumbers[i] = string.Join(',', randomIds);
        }
    }

    /// <summary>
    /// Ye olden way of doing it (before 20250201 https://github.com/umbraco/Umbraco-CMS/pull/18048)
    /// </summary>
    /// <returns>A number so the compiler/runtime doesn't optimize it away.</returns>
    [Benchmark]
    public int Linq()
    {
        var totalNumberOfIds = 0; // a number to operate on so it is not optimized away
        foreach (string? stringWithCommaSeparatedNumbers in _stringsWithCommaSeparatedNumbers)
        {
            totalNumberOfIds += Linq(stringWithCommaSeparatedNumbers).Length;
        }

        return totalNumberOfIds;
    }

    private static int[] Linq(string path)
    {
        int[]? nodeIds = path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
                int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var output)
                    ? Attempt<int>.Succeed(output)
                    : Attempt<int>.Fail())
            .Where(x => x.Success)
            .Select(x => x.Result)
            .Reverse()
            .ToArray();
        return nodeIds;
    }

    /// <summary>
    /// Here we are allocating strings to the separated values,
    /// BUT we know the count of numbers, so we can allocate the exact size of list we need
    /// </summary>
    /// <returns>A number so the compiler/runtime doesn't optimize it away.</returns>
    [Benchmark]
    public int SplitToHeapStrings()
    {
        var totalNumberOfIds = 0; // a number to operate on so it is not optimized away
        foreach (string stringWithCommaSeparatedNumbers in _stringsWithCommaSeparatedNumbers)
        {
            totalNumberOfIds += SplitToHeapStrings(stringWithCommaSeparatedNumbers).Length;
        }

        return totalNumberOfIds;
    }

    private static int[] SplitToHeapStrings(string path)
    {
        string[] pathSegments = path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        List<int> nodeIds = new(pathSegments.Length); // here we know how large the resulting list should at least be
        for (int i = pathSegments.Length - 1; i >= 0; i--)
        {
            if (int.TryParse(pathSegments[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        return nodeIds.ToArray(); // allocates a new array
    }

    /// <summary>
    /// Here we avoid allocating strings to the separated values,
    /// BUT we do not know the count of numbers, so we might end up resizing the list we add numbers to it
    /// </summary>
    /// <returns>A number so the compiler/runtime doesn't optimize it away.</returns>
    [Benchmark]
    public int SplitToStackSpansWithoutEmptyCheckReversingListAsSpan()
    {
        var totalNumberOfIds = 0; // a number to operate on so it is not optimized away
        foreach (string stringWithCommaSeparatedNumbers in _stringsWithCommaSeparatedNumbers)
        {
            totalNumberOfIds += SplitToStackSpansWithoutEmptyCheckReversingListAsSpan(stringWithCommaSeparatedNumbers).Length;
        }

        return totalNumberOfIds;
    }

    private static int[] SplitToStackSpansWithoutEmptyCheckReversingListAsSpan(string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();
        MemoryExtensions.SpanSplitEnumerator<char> pathSegments = pathSpan.Split(Constants.CharArrays.Comma);

        // Here we do NOT know how large the resulting list should at least be
        // Default empty List<> internal array capacity on add is currently 4
        // If the count of numbers are less than 4, we overallocate a little
        // If the count of numbers are more than 4, the list will be resized, resulting in a copy from initial array to new double size array
        // If the count of numbers are more than 8, another new array is allocated and copied to
        List<int> nodeIds = [];
        foreach (Range rangeOfPathSegment in pathSegments)
        {
            // this is only a span of the string, a string is not allocated on the heap
            ReadOnlySpan<char> pathSegmentSpan = pathSpan[rangeOfPathSegment];
            if (int.TryParse(pathSegmentSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        Span<int> nodeIdsSpan = CollectionsMarshal.AsSpan(nodeIds);
        var result = new int[nodeIdsSpan.Length];
        var resultIndex = 0;
        for (int i = nodeIdsSpan.Length - 1; i >= 0; i--)
        {
            result[resultIndex++] = nodeIdsSpan[i];
        }

        return result;
    }

    /// <summary>
    /// Here we avoid allocating strings to the separated values,
    /// BUT we do not know the count of numbers, so we might end up resizing the list we add numbers to it
    /// </summary>
    /// <returns>A number so the compiler/runtime doesn't optimize it away.</returns>
    [Benchmark]
    public int SplitToStackSpansWithoutEmptyCheck()
    {
        var totalNumberOfIds = 0; // a number to operate on so it is not optimized away
        foreach (string stringWithCommaSeparatedNumbers in _stringsWithCommaSeparatedNumbers)
        {
            totalNumberOfIds += SplitToStackSpansWithoutEmptyCheck(stringWithCommaSeparatedNumbers).Length;
        }

        return totalNumberOfIds;
    }

    private static int[] SplitToStackSpansWithoutEmptyCheck(string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();
        MemoryExtensions.SpanSplitEnumerator<char> pathSegments = pathSpan.Split(Constants.CharArrays.Comma);

        // Here we do NOT know how large the resulting list should at least be
        // Default empty List<> internal array capacity on add is currently 4
        // If the count of numbers are less than 4, we overallocate a little
        // If the count of numbers are more than 4, the list will be resized, resulting in a copy from initial array to new double size array
        // If the count of numbers are more than 8, another new array is allocated and copied to
        List<int> nodeIds = [];
        foreach (Range rangeOfPathSegment in pathSegments)
        {
            // this is only a span of the string, a string is not allocated on the heap
            ReadOnlySpan<char> pathSegmentSpan = pathSpan[rangeOfPathSegment];
            if (int.TryParse(pathSegmentSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        var result = new int[nodeIds.Count];
        var resultIndex = 0;
        for (int i = nodeIds.Count - 1; i >= 0; i--)
        {
            result[resultIndex++] = nodeIds[i];
        }

        return result;
    }

    /// <summary>
    /// Here we avoid allocating strings to the separated values,
    /// BUT we do not know the count of numbers, so we might end up resizing the list we add numbers to it
    /// </summary>
    /// <remarks>Here with an empty check, unlikely in umbraco use case.</remarks>
    /// <returns>A number so the compiler/runtime doesn't optimize it away.</returns>
    [Benchmark]
    public int SplitToStackSpansWithEmptyCheck()
    {
        var totalNumberOfIds = 0; // a number to operate on so it is not optimized away
        foreach (string stringWithCommaSeparatedNumbers in _stringsWithCommaSeparatedNumbers)
        {
            totalNumberOfIds += SplitToStackSpansWithEmptyCheck(stringWithCommaSeparatedNumbers).Length;
        }

        return totalNumberOfIds;
    }

    private static int[] SplitToStackSpansWithEmptyCheck(string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();
        MemoryExtensions.SpanSplitEnumerator<char> pathSegments = pathSpan.Split(Constants.CharArrays.Comma);

        // Here we do NOT know how large the resulting list should at least be
        // Default empty List<> internal array capacity on add is currently 4
        // If the count of numbers are less than 4, we overallocate a little
        // If the count of numbers are more than 4, the list will be resized, resulting in a copy from initial array to new double size array
        // If the count of numbers are more than 8, another new array is allocated and copied to
        List<int> nodeIds = [];
        foreach (Range rangeOfPathSegment in pathSegments)
        {
            // this is only a span of the string, a string is not allocated on the heap
            ReadOnlySpan<char> pathSegmentSpan = pathSpan[rangeOfPathSegment];
            if (pathSegmentSpan.IsEmpty)
            {
                continue;
            }

            if (int.TryParse(pathSegmentSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        var result = new int[nodeIds.Count];
        var resultIndex = 0;
        for (int i = nodeIds.Count - 1; i >= 0; i--)
        {
            result[resultIndex++] = nodeIds[i];
        }

        return result;
    }

// BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.2894)
// Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
// .NET Core SDK 3.1.426 [C:\Program Files\dotnet\sdk]
//   [Host] : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
//
// Toolchain=InProcessEmitToolchain
//
// | Method                                                | Mean     | Error    | StdDev   | Gen0   | Allocated |
// |------------------------------------------------------ |---------:|---------:|---------:|-------:|----------:|
// | Linq                                                  | 46.39 us | 0.515 us | 0.430 us | 9.4604 |  58.31 KB |
// | SplitToHeapStrings                                    | 30.28 us | 0.310 us | 0.275 us | 7.0801 |  43.55 KB |
// | SplitToStackSpansWithoutEmptyCheckReversingListAsSpan | 20.47 us | 0.290 us | 0.257 us | 2.7161 |  16.73 KB |
// | SplitToStackSpansWithoutEmptyCheck                    | 20.60 us | 0.315 us | 0.280 us | 2.7161 |  16.73 KB |
// | SplitToStackSpansWithEmptyCheck                       | 20.57 us | 0.308 us | 0.288 us | 2.7161 |  16.73 KB |
}
