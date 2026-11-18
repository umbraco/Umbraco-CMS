using BenchmarkDotNet.Attributes;
using Moq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures resolving the domains assigned to a node, which URL generation does for every ancestor of the content.
/// Most content has no domain assigned, so the common case is a node that matches none of the cached domains.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*DomainCacheServiceBenchmarks*"</c>.
/// Compare a run on the branch with a run on the branch it was cut from.
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class DomainCacheServiceBenchmarks
{
    private const int NodeWithoutDomain = 1;
    private const int NodeWithOneDomain = 2;

    private DomainCacheService _sut = null!;

    [Params(1, 20)]
    public int DomainCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var domains = new List<IDomain>();
        for (var i = 0; i < DomainCount; i++)
        {
            // Domains assigned to other nodes, one of them to the node used for the single match.
            domains.Add(CreateDomain(i + 1, $"example{i}.com", i == 0 ? NodeWithOneDomain : 1000 + i));
        }

        var domainService = new Mock<IDomainService>();
        domainService.Setup(x => x.GetAllAsync(true)).ReturnsAsync(domains);

        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider.Setup(x => x.CreateCoreScope()).Returns(Mock.Of<ICoreScope>());

        _sut = new DomainCacheService(domainService.Object, scopeProvider.Object);

        // The first call populates the cache.
        _ = _sut.GetAssigned(NodeWithoutDomain);
    }

    [Benchmark]
    public bool GetAssigned_NodeWithoutDomain()
        => _sut.GetAssigned(NodeWithoutDomain).Any();

    [Benchmark]
    public bool HasAssigned_NodeWithoutDomain()
        => _sut.HasAssigned(NodeWithoutDomain);

    [Benchmark]
    public bool GetAssigned_NodeWithOneDomain()
        => _sut.GetAssigned(NodeWithOneDomain).Any();

    private static IDomain CreateDomain(int id, string name, int rootContentId)
    {
        var domain = new Mock<IDomain>();
        domain.SetupGet(x => x.Id).Returns(id);
        domain.SetupGet(x => x.DomainName).Returns(name);
        domain.SetupGet(x => x.RootContentId).Returns(rootContentId);
        domain.SetupGet(x => x.LanguageIsoCode).Returns("en-US");
        domain.SetupGet(x => x.IsWildcard).Returns(false);
        domain.SetupGet(x => x.SortOrder).Returns(0);
        return domain.Object;
    }
}
