using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class CollectionBenchmarks
    {
        private const int Size = 1000;
        private static readonly IEnumerable<int> _IReadOnlyListEnumerable = Enumerable.Range(0, Size);
        private static readonly int[] _array = _IReadOnlyListEnumerable.ToArray();
        private static readonly List<int> _list = _IReadOnlyListEnumerable.ToList();

        [Benchmark]
        public int[] IReadOnlyListToArray()
        {
            return _IReadOnlyListEnumerable.ToArray();
        }

        [Benchmark]
        public List<int> IReadOnlyListToList()
        {
            return _IReadOnlyListEnumerable.ToList();
        }

        // LINQ Where and many other enumerables does not know how many items there will be returned
        private static IEnumerable<int> YieldingEnumerable()
        {
            for (int i = 0; i < Size; i++)
            {
                yield return i;
            }
        }

        [Benchmark]
        public int[] YieldingEnumerableToArray()
        {
            return YieldingEnumerable().ToArray();
        }

        [Benchmark]
        public List<int> YieldingEnumerableToList()
        {
            return YieldingEnumerable().ToList();
        }

        [Benchmark]
        public void IterateArray()
        {
            foreach (int item in _array)
            {

            }
        }

        [Benchmark]
        public void IterateAsSpanList()
        {
            foreach (int item in CollectionsMarshal.AsSpan(_list))
            {
            }
        }

        [Benchmark]
        public void IterateList()
        {
            foreach (int item in _list)
            {
            }
        }

        //BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.4169/23H2/2023Update/SunValley3)
        //Intel Core i7-10750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
        //.NET SDK 8.0.401
        //  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
        //  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
        //
        //
        //| Method                    | Mean       | Error    | StdDev   | Gen0   | Gen1   | Allocated |
        //|-------------------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
        //| IReadOnlyListToArray      |   227.2 ns |  4.35 ns |  4.46 ns | 0.6413 |      - |    4024 B |
        //| IReadOnlyListToList       |   236.8 ns |  3.77 ns |  3.53 ns | 0.6464 | 0.0098 |    4056 B |
        //| YieldingEnumerableToArray | 3,249.0 ns | 63.49 ns | 96.96 ns | 1.3580 | 0.0114 |    8528 B |
        //| YieldingEnumerableToList  | 2,791.3 ns | 54.36 ns | 76.21 ns | 1.3466 | 0.0229 |    8456 B |
        //| IterateArray              |   245.3 ns |  4.09 ns |  3.42 ns |      - |      - |         - |
        //| IterateAsSpanList         |   249.9 ns |  3.10 ns |  2.90 ns |      - |      - |         - |
        //| IterateList               |   543.2 ns | 10.86 ns | 17.84 ns |      - |      - |         - |
    }
}
