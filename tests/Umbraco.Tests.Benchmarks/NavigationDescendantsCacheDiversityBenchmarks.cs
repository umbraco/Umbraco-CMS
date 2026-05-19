using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Moq;
using Perfolizer.Horology;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Documents the cache shape under multi-tenant or heterogeneous-content-type workloads:
/// the per-snapshot <c>DescendantsCache</c> is keyed by <c>(parent, contentType?)</c>, so a parent
/// queried with N different <c>Descendants("typeX")</c> aliases accumulates up to N entries. This
/// benchmark sweeps the content-type-diversity parameter so the steady-state allocation and per-
/// query time can be compared as the per-parent cache fan-out grows.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*NavigationDescendantsCacheDiversity*"</c>.
/// The benchmark uses a plain <see cref="DocumentNavigationService"/> rather than the full HybridCache
/// stack — the cache lives on the navigation snapshot itself, so the HybridCache layer is irrelevant
/// to what we're measuring here.
/// </remarks>
[Config(typeof(InProcessStableRunConfig))]
public class NavigationDescendantsCacheDiversityBenchmarks
{
    private sealed class InProcessStableRunConfig : ManualConfig
    {
        public InProcessStableRunConfig()
        {
            AddJob(Job.Default
                .WithLaunchCount(1)
                .WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithToolchain(InProcessEmitToolchain.Instance));
            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }

    /// <summary>
    /// Number of distinct content types spread across the tree.
    /// </summary>
    [Params(5, 25, 50)]
    public int ContentTypeCount { get; set; }

    [Params(10)]
    public int ParentCount { get; set; }

    [Params(20)]
    public int ChildrenPerParentPerType { get; set; }

    private DocumentNavigationService _service = null!;
    private Guid[] _parentKeys = null!;
    private string[] _contentTypeAliases = null!;

    [GlobalSetup]
    public void Setup()
    {
        _contentTypeAliases = new string[ContentTypeCount];
        var contentTypeKeys = new Guid[ContentTypeCount];
        var contentTypes = new IContentType[ContentTypeCount];
        for (var i = 0; i < ContentTypeCount; i++)
        {
            _contentTypeAliases[i] = $"type{i}";
            contentTypeKeys[i] = Guid.NewGuid();

            var ct = new Mock<IContentType>();
            ct.SetupGet(x => x.Alias).Returns(_contentTypeAliases[i]);
            ct.SetupGet(x => x.Key).Returns(contentTypeKeys[i]);
            contentTypes[i] = ct.Object;
        }

        var contentTypeService = new Mock<IContentTypeService>();
        contentTypeService.Setup(s => s.GetAll()).Returns(contentTypes);

        _service = new DocumentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            contentTypeService.Object);

        // Build a flat-ish tree: ParentCount root parents, each with (ContentTypeCount × ChildrenPerParentPerType) children.
        // The first content type "owns" each parent so the parents themselves are queryable.
        _parentKeys = new Guid[ParentCount];
        for (var p = 0; p < ParentCount; p++)
        {
            Guid parentKey = Guid.NewGuid();
            _parentKeys[p] = parentKey;
            _service.Add(parentKey, contentTypeKeys[0]);

            for (var t = 0; t < ContentTypeCount; t++)
            {
                for (var c = 0; c < ChildrenPerParentPerType; c++)
                {
                    _service.Add(Guid.NewGuid(), contentTypeKeys[t], parentKey);
                }
            }
        }
    }

    /// <summary>
    /// One full sweep: every parent queried for every content type. After the first iteration the
    /// cache is fully primed; subsequent iterations should be cache hits with near-zero allocation.
    /// The Δ between iteration 1 and the steady state reveals the per-entry build cost.
    /// </summary>
    [Benchmark]
    public int FullSweep_AllParentsAllTypes()
    {
        var total = 0;
        for (var p = 0; p < ParentCount; p++)
        {
            for (var t = 0; t < ContentTypeCount; t++)
            {
                if (_service.TryGetDescendantsKeysOfType(_parentKeys[p], _contentTypeAliases[t], out IEnumerable<Guid> keys))
                {
                    total += keys.Count();
                }
            }
        }

        return total;
    }

    /// <summary>
    /// Repeated unfiltered <c>Descendants()</c> calls on a single parent. The cache key is
    /// <c>(parent, null)</c> so this is a single entry; the benchmark validates that diverse-type
    /// fan-out on neighbouring parents does not pollute or evict this entry.
    /// </summary>
    [Benchmark]
    public int RepeatedUnfilteredOnSingleParent()
    {
        var total = 0;
        for (var i = 0; i < 100; i++)
        {
            if (_service.TryGetDescendantsKeys(_parentKeys[0], out IEnumerable<Guid> keys))
            {
                total += keys.Count();
            }
        }

        return total;
    }
}
