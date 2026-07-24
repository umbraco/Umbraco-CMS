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
/// Media counterpart to <see cref="HybridCacheColdNavigationBenchmarks"/> — the reported workload is a
/// large, flat media library, so this measures cold <c>Children()</c> / <c>Descendants()</c> over media.
/// See that type's remarks for how cold measurement, latency modelling and the fetch counters work.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*HybridCacheColdMediaNavigation*"</c>.
/// </remarks>
[Config(typeof(ColdMediaRunConfig))]
public class HybridCacheColdMediaNavigationBenchmarks
{
    private sealed class ColdMediaRunConfig : ManualConfig
    {
        public ColdMediaRunConfig()
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

    // Fixed tree shape (≈ 5,051 nodes); not a [Params] since there is only one value to sweep.
    private const int BranchCount = 50;
    private const int LeafCount = 100;

    private SyntheticPublishedMediaTreeFixture _fixture = null!;

    [Params(0, 1)]
    public int RepoLatencyMs { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _fixture = new SyntheticPublishedMediaTreeFixture();
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
