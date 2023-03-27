using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class CollectionBenchmarks
    {
        private static readonly IEnumerable<int> _enumerable = Enumerable.Range(0, 1000);
        private static readonly int[] _array = _enumerable.ToArray();
        private static readonly List<int> _list = _enumerable.ToList();

        [Benchmark]
        public int[] ToArray()
        {
            return _enumerable.ToArray();
        }

        [Benchmark]
        public List<int> ToList()
        {
            return _enumerable.ToList();
        }

        [Benchmark]
        public void IterateArray()
        {
            foreach (int item in _array)
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

        //BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.2006 (21H2)
        //Intel Core i9-10885H CPU 2.40GHz, 1 CPU, 16 logical and 8 physical cores
        //.NET SDK= 6.0.401
        //  [Host]     : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT
        //  DefaultJob : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT
        //
        //
        //|       Method |       Mean |    Error |   StdDev |  Gen 0 |  Gen 1 | Allocated |
        //|------------- |-----------:|---------:|---------:|-------:|-------:|----------:|
        //|      ToArray |   503.8 ns |  5.11 ns |  4.53 ns | 0.4807 | 0.0067 |   4,024 B |
        //|       ToList | 1,369.0 ns | 25.38 ns | 49.50 ns | 0.4845 | 0.0134 |   4,056 B |
        //| IterateArray |   244.9 ns |  3.29 ns |  2.75 ns |      - |      - |         - |
        //|  IterateList |   620.5 ns |  4.45 ns |  3.95 ns |      - |      - |         - |
    }
}
