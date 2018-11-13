using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Umbraco.Core;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class TryConvertToBenchmarks
    {
        private static readonly List<string> List = new List<string>() { "hello", "world", "awesome" };
        private static readonly string Date = "Saturday 10, November 2012";

        [Benchmark(Description = "List<string> to IEnumerable<string>")]
        public IList<string> TryConvertToEnumerable()
        {
            return List.TryConvertTo<IEnumerable<string>>().Result.ToList();
        }

        [Benchmark(Description = "Int to Double")]
        public double TryConvertToDouble()
        {
            return 1.TryConvertTo<double>().Result;
        }

        [Benchmark(Description = "Float to Decimal")]
        public decimal TryConvertToDecimal()
        {
            return 1F.TryConvertTo<decimal>().Result;
        }

        [Benchmark(Description = "String to Boolean")]
        public bool TryConvertToBoolean()
        {
            return "1".TryConvertTo<bool>().Result;
        }

        [Benchmark(Description = "String to DateTime")]
        public DateTime TryConvertToDateTime()
        {
            return Date.TryConvertTo<DateTime>().Result;
        }
    }
}
