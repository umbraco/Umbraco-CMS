using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Tests.Benchmarks;

[MemoryDiagnoser]
public class TryConvertToBenchmarks
{
    private static readonly List<string> List = new() { "hello", "world", "awesome" };
    private static readonly string Date = "Saturday 10, November 2012";

    [Benchmark(Description = "List<string> to IEnumerable<string>")]
    public IList<string> TryConvertToEnumerable() => List.TryConvertTo<IEnumerable<string>>().Result.ToList();

    [Benchmark(Description = "Int to Double")]
    public double TryConvertToDouble() => 1.TryConvertTo<double>().Result;

    [Benchmark(Description = "Float to Decimal")]
    public decimal TryConvertToDecimal() => 1F.TryConvertTo<decimal>().Result;

    [Benchmark(Description = "String to Boolean")]
    public bool TryConvertToBoolean() => "1".TryConvertTo<bool>().Result;

    [Benchmark(Description = "String to DateTime")]
    public DateTime TryConvertToDateTime() => Date.TryConvertTo<DateTime>().Result;
}
