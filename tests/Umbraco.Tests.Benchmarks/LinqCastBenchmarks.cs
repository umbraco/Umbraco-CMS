﻿using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Umbraco.Tests.Benchmarks
{
    /// <summary>
    /// Want to check what is faster OfType or Cast when a enurable all has the same items
    /// </summary>
    [MemoryDiagnoser]
    public class LinqCastBenchmarks
    {
        public LinqCastBenchmarks()
        {
            _array = new List<object>();
            _array.AddRange(Enumerable.Range(0, 10000).Select(x => x.ToString()));
        }

        private readonly List<object> _array;

        [Benchmark(Baseline = true)]
        public void OfType()
        {
            foreach (var i in _array.OfType<string>())
            {
                var a = i;
            }
        }

        [Benchmark()]
        public void Cast()
        {
            foreach (var i in _array.Cast<string>())
            {
                var a = i;
            }
        }

        [Benchmark()]
        public void ExplicitCast()
        {
            foreach (var i in _array)
            {
                var a = (string)i;
            }
        }
    }
}
