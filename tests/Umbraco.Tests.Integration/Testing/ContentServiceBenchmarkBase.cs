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

    // Regression enforcement configuration
    private const double DefaultRegressionThreshold = 20.0;

    // Allow CI override via environment variable
    private static readonly double RegressionThreshold =
        double.TryParse(Environment.GetEnvironmentVariable("BENCHMARK_REGRESSION_THRESHOLD"), out var t)
            ? t
            : DefaultRegressionThreshold;

    // Optional strict mode: fail if baseline is missing (useful for CI)
    private static readonly bool RequireBaseline =
        bool.TryParse(Environment.GetEnvironmentVariable("BENCHMARK_REQUIRE_BASELINE"), out var b) && b;

    // Thread-safe lazy initialization of repository root
    private static readonly Lazy<string> _repositoryRoot = new(FindRepositoryRoot);

    // Thread-safe lazy initialization of baseline data
    private static readonly Lazy<Dictionary<string, BenchmarkResult>> _baselineLoader =
        new(() => LoadBaselineInternal(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static Dictionary<string, BenchmarkResult> Baseline => _baselineLoader.Value;

    private static string BaselinePath => Path.Combine(_repositoryRoot.Value, "docs", "plans", "baseline-phase0.json");

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
    /// Finds the repository root by searching for umbraco.sln.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "umbraco.sln")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            $"Cannot find repository root (umbraco.sln) starting from {TestContext.CurrentContext.TestDirectory}");
    }

    /// <summary>
    /// Records a benchmark and asserts no regression beyond the threshold.
    /// </summary>
    /// <param name="name">Benchmark name (must match baseline JSON key).</param>
    /// <param name="elapsedMs">Measured elapsed time in milliseconds.</param>
    /// <param name="itemCount">Number of items processed.</param>
    /// <param name="thresholdPercent">Maximum allowed regression percentage (default: 20%, configurable via BENCHMARK_REGRESSION_THRESHOLD env var).</param>
    protected void AssertNoRegression(string name, long elapsedMs, int itemCount, double thresholdPercent = -1)
    {
        RecordBenchmark(name, elapsedMs, itemCount);

        // Use environment-configurable threshold if not explicitly specified
        var effectiveThreshold = thresholdPercent < 0 ? RegressionThreshold : thresholdPercent;

        if (Baseline.TryGetValue(name, out var baselineResult))
        {
            var maxAllowed = baselineResult.ElapsedMs * (1 + effectiveThreshold / 100);

            if (elapsedMs > maxAllowed)
            {
                var regressionPct = ((double)(elapsedMs - baselineResult.ElapsedMs) / baselineResult.ElapsedMs) * 100;
                Assert.Fail(
                    $"Performance regression detected for '{name}': " +
                    $"{elapsedMs}ms exceeds threshold of {maxAllowed:F0}ms " +
                    $"(baseline: {baselineResult.ElapsedMs}ms, regression: +{regressionPct:F1}%, threshold: {effectiveThreshold}%)");
            }

            TestContext.WriteLine($"[REGRESSION_CHECK] {name}: PASS ({elapsedMs}ms <= {maxAllowed:F0}ms, baseline: {baselineResult.ElapsedMs}ms, threshold: {effectiveThreshold}%)");
        }
        else if (RequireBaseline)
        {
            Assert.Fail($"No baseline entry found for '{name}' and BENCHMARK_REQUIRE_BASELINE=true");
        }
        else
        {
            TestContext.WriteLine($"[REGRESSION_CHECK] {name}: SKIPPED (no baseline entry)");
        }
    }

    /// <summary>
    /// Measures, records, and asserts no regression for the given action.
    /// </summary>
    protected long MeasureAndAssertNoRegression(string name, int itemCount, Action action, bool skipWarmup = false, double thresholdPercent = -1)
    {
        // Warmup iteration (skip for destructive operations)
        if (!skipWarmup)
        {
            try { action(); }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[WARMUP] {name} warmup failed: {ex.Message}");
            }
        }

        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();

        AssertNoRegression(name, sw.ElapsedMilliseconds, itemCount, thresholdPercent);
        return sw.ElapsedMilliseconds;
    }

    private static Dictionary<string, BenchmarkResult> LoadBaselineInternal()
    {
        if (!File.Exists(BaselinePath))
        {
            TestContext.WriteLine($"[BASELINE] File not found: {BaselinePath}");
            return new Dictionary<string, BenchmarkResult>();
        }

        try
        {
            var json = File.ReadAllText(BaselinePath);
            var results = JsonSerializer.Deserialize<List<BenchmarkResult>>(json) ?? new List<BenchmarkResult>();
            TestContext.WriteLine($"[BASELINE] Loaded {results.Count} baseline entries from {BaselinePath}");
            return results.ToDictionary(r => r.Name, r => r);
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"[BASELINE] Failed to load baseline: {ex.Message}");
            return new Dictionary<string, BenchmarkResult>();
        }
    }

    /// <summary>
    /// Represents a single benchmark measurement.
    /// </summary>
    internal sealed record BenchmarkResult(string Name, long ElapsedMs, int ItemCount)
    {
        public double MsPerItem => ItemCount > 0 ? (double)ElapsedMs / ItemCount : 0;
    }
}
