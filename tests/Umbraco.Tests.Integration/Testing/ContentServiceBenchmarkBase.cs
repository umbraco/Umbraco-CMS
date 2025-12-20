// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
/// Base class for ContentService performance benchmarks.
/// Extends UmbracoIntegrationTestWithContent with structured benchmark recording.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// [Test]
/// [LongRunning]
/// public void MyBenchmark()
/// {
///     var sw = Stopwatch.StartNew();
///     // ... operation under test ...
///     sw.Stop();
///     RecordBenchmark("MyBenchmark", sw.ElapsedMilliseconds, itemCount);
/// }
/// </code>
///
/// Results are output in both human-readable and JSON formats for baseline comparison.
/// </remarks>
public abstract class ContentServiceBenchmarkBase : UmbracoIntegrationTestWithContent
{
    private readonly List<BenchmarkResult> _results = new();

    /// <summary>
    /// Records a benchmark result for later output.
    /// </summary>
    /// <param name="name">Name of the benchmark (should match method name).</param>
    /// <param name="elapsedMs">Elapsed time in milliseconds.</param>
    /// <param name="itemCount">Number of items processed (for per-item metrics).</param>
    protected void RecordBenchmark(string name, long elapsedMs, int itemCount)
    {
        var result = new BenchmarkResult(name, elapsedMs, itemCount);
        _results.Add(result);

        // Human-readable output
        TestContext.WriteLine($"[BENCHMARK] {name}: {elapsedMs}ms ({result.MsPerItem:F2}ms/item, {itemCount} items)");
    }

    /// <summary>
    /// Records a benchmark result without item count (for single-item operations).
    /// </summary>
    protected void RecordBenchmark(string name, long elapsedMs)
        => RecordBenchmark(name, elapsedMs, 1);

    /// <summary>
    /// Measures and records a benchmark for the given action.
    /// </summary>
    /// <param name="name">Name of the benchmark.</param>
    /// <param name="itemCount">Number of items processed.</param>
    /// <param name="action">The action to benchmark.</param>
    /// <param name="skipWarmup">Skip warmup for destructive operations (delete, empty recycle bin).</param>
    /// <returns>Elapsed time in milliseconds.</returns>
    protected long MeasureAndRecord(string name, int itemCount, Action action, bool skipWarmup = false)
    {
        // Warmup iteration: triggers JIT compilation, warms connection pool and caches.
        // Skip for destructive operations that would fail on second execution.
        if (!skipWarmup)
        {
            try
            {
                action();
            }
            catch
            {
                // Warmup failure is acceptable for some operations; continue to measured run
            }
        }

        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        RecordBenchmark(name, sw.ElapsedMilliseconds, itemCount);
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Measures and records a benchmark, returning the result of the function.
    /// </summary>
    /// <remarks>
    /// Performs a warmup call before measurement to trigger JIT compilation.
    /// Safe for read-only operations that can be repeated without side effects.
    /// </remarks>
    protected T MeasureAndRecord<T>(string name, int itemCount, Func<T> func)
    {
        // Warmup: triggers JIT compilation, warms caches
        try { func(); } catch { /* ignore warmup errors */ }

        var sw = Stopwatch.StartNew();
        var result = func();
        sw.Stop();
        RecordBenchmark(name, sw.ElapsedMilliseconds, itemCount);
        return result;
    }

    [TearDown]
    public void OutputBenchmarkResults()
    {
        if (_results.Count == 0)
        {
            return;
        }

        // JSON output for automated comparison
        // Wrapped in markers for easy extraction from test output
        var json = JsonSerializer.Serialize(_results, new JsonSerializerOptions { WriteIndented = true });
        TestContext.WriteLine($"[BENCHMARK_JSON]{json}[/BENCHMARK_JSON]");

        _results.Clear();
    }

    /// <summary>
    /// Represents a single benchmark measurement.
    /// </summary>
    internal sealed record BenchmarkResult(string Name, long ElapsedMs, int ItemCount)
    {
        public double MsPerItem => ItemCount > 0 ? (double)ElapsedMs / ItemCount : 0;
    }
}
