using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Perfolizer.Horology;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Umbraco.Tests.Benchmarks.Fixtures;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures the cost of <c>Children()</c> and <c>Descendants()</c> on <see cref="IPublishedContent"/>
/// against the published-content cache, using a known synthetic tree shape.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*HybridCacheNavigation*"</c>.
/// Uses <see cref="InProcessEmitToolchain"/> so each benchmark runs in the same process via
/// <c>Reflection.Emit</c>, avoiding the per-benchmark MSBuild compile (which otherwise both times
/// out and floods the console with the repo's pre-existing warnings).
/// </remarks>
[Config(typeof(InProcessStableRunConfig))]
public class HybridCacheNavigationBenchmarks
{
    /// <summary>
    /// Longer iterations than <c>QuickRun</c> so the variance on the multi-thousand-node descendant
    /// benchmarks settles enough to support before/after comparisons. <c>InProcessEmitToolchain</c>
    /// keeps the run in the same process to avoid the per-benchmark MSBuild step.
    /// </summary>
    private sealed class InProcessStableRunConfig : ManualConfig
    {
        public InProcessStableRunConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                .WithWarmupCount(5)
                .WithIterationCount(10)
                .WithToolchain(InProcessEmitToolchain.Instance));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    private SyntheticPublishedTreeFixture _fixture = null!;

    /// <summary>
    /// Approximate total node count: <c>1 + BranchCount + (BranchCount * LeafCount)</c>.
    /// The single 50 × 100 = 5,051-node configuration is the case closest to the reported workload;
    /// it's enough to see the impact of the targeted improvements without paying for the full matrix
    /// each iterate-and-compare cycle. Add more <c>[Params]</c> values when producing the final
    /// PR-description numbers if scaling behaviour matters.
    /// </summary>
    [Params(50)]
    public int BranchCount { get; set; }

    [Params(100)]
    public int LeafCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _fixture = new SyntheticPublishedTreeFixture();
        _fixture.InitialiseAsync(BranchCount, LeafCount).GetAwaiter().GetResult();

        // Warm the converted-content cache so each benchmark measures the steady-state
        // hot path rather than first-touch materialisation.
        _ = _fixture.Root
            .Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .Count();
    }

    [Benchmark]
    public int Children_Count()
        => _fixture.Root
            .Children(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .Count();

    [Benchmark]
    public int Children_ReadOneProperty()
    {
        var read = 0;
        foreach (IPublishedContent child in _fixture.Root.Children(_fixture.NavigationQueryService, _fixture.StatusFilteringService))
        {
            // Touch one property per child so we exercise the property pipeline once.
            _ = child.Value<string>(_fixture.PublishedValueFallback, "prop0");
            read++;
        }

        return read;
    }

    [Benchmark]
    public int Descendants_Count()
        => _fixture.Root
            .Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .Count();

    [Benchmark]
    public int Descendants_ReadOneProperty()
    {
        var read = 0;
        foreach (IPublishedContent descendant in _fixture.Root.Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService))
        {
            _ = descendant.Value<string>(_fixture.PublishedValueFallback, "prop0");
            read++;
        }

        return read;
    }

    /// <summary>
    /// Asks for only the first descendant. Reveals whether enumeration short-circuits or
    /// materialises the full descendant set before yielding.
    /// </summary>
    [Benchmark]
    public IPublishedContent? Descendants_FirstOrDefault()
        => _fixture.Root
            .Descendants(_fixture.NavigationQueryService, _fixture.StatusFilteringService)
            .FirstOrDefault();

    /// <summary>
    /// Recursive traversal pattern that calls Children() twice per node — once for Any() and once
    /// for the recursive step.
    /// </summary>
    [Benchmark]
    public int RecursiveTraversal()
        => GetDescendants([_fixture.Root]).Count;

    private List<IPublishedContent> GetDescendants(IEnumerable<IPublishedContent> contents)
    {
        var result = contents.ToList();
        result
            .Where(x => x.Children(_fixture.NavigationQueryService, _fixture.StatusFilteringService).Any())
            .ToList()
            .ForEach(x => result.AddRange(GetDescendants(x.Children(_fixture.NavigationQueryService, _fixture.StatusFilteringService))));
        return result;
    }
}
