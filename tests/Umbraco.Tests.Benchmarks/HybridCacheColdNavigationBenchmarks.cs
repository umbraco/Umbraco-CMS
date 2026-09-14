using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Umbraco.Tests.Benchmarks.Fixtures;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures <c>Children()</c> / <c>Descendants()</c> against a COLD published-content cache, where the
/// items are absent from both the converted (L0) and HybridCache (L1) tiers and must be read from the
/// repository. This is the case the batched read-through targets: a per-key path issues one repository
/// round trip per item, whereas the batched path collapses them into a handful of reads.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*HybridCacheColdNavigation*"</c>.
/// <para>
/// Each measured invocation is made cold by <see cref="ResetCold"/> (an <c>[IterationSetup]</c>) with
/// <c>InvocationCount = 1</c>, so the reported time is a single cold traversal. <c>RepoLatencyMs</c>
/// models database round-trip latency; at a realistic latency the difference between one-per-item and
/// batched reads dominates. The fixture also exposes <c>SingleFetchCount</c> / <c>BatchFetchCount</c>
/// which are written to the console after each iteration as the hardware-independent, deterministic
/// signal (per-item path → many single fetches; batched path → a few batched fetches).
/// </para>
/// </remarks>
[Config(typeof(ColdRunConfig))]
public class HybridCacheColdNavigationBenchmarks
{
    private sealed class ColdRunConfig : ManualConfig
    {
        public ColdRunConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithWarmupCount(3)
                .WithIterationCount(10)
                .WithInvocationCount(1)
                .WithUnrollFactor(1)
                .WithToolchain(InProcessEmitToolchain.Instance));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    private SyntheticPublishedDocumentTreeFixture _fixture = null!;

    // Fixed tree shape (≈ 5,051 nodes); not a [Params] since there is only one value to sweep.
    private const int BranchCount = 50;
    private const int LeafCount = 100;

    [Params(0, 1)]
    public int RepoLatencyMs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _fixture = new SyntheticPublishedDocumentTreeFixture();
        _fixture.InitialiseAsync(BranchCount, LeafCount, seed: false, repoLatencyMs: RepoLatencyMs).GetAwaiter().GetResult();
    }

    [IterationSetup]
    public void ResetCold() => _fixture.ResetColdAsync().GetAwaiter().GetResult();

    [IterationCleanup]
    public void ReportFetches()
        => Console.WriteLine($"[fetches] single={_fixture.SingleFetchCount} batched={_fixture.BatchFetchCount}");

    [Benchmark]
    public int Descendants_Count()
        => _fixture.Root
            .Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .Count();

    [Benchmark]
    public IPublishedContent? Descendants_FirstOrDefault()
        => _fixture.Root
            .Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .FirstOrDefault();

    [Benchmark]
    public int Children_Count()
        => _fixture.Root
            .Children(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .Count();
}
