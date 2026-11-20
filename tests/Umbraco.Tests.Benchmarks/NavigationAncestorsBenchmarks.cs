using BenchmarkDotNet.Attributes;
using Moq;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
/// Measures getting the ancestor keys of a document from the navigation structure, which URL generation and the
/// published-status checks do once or more per URL.
/// </summary>
/// <remarks>
/// Run with: <c>dotnet run -c Release --project tests/Umbraco.Tests.Benchmarks -- --filter "*NavigationAncestorsBenchmarks*"</c>.
/// Compare a run on the branch with a run on the branch it was cut from.
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class NavigationAncestorsBenchmarks
{
    private IDocumentNavigationQueryService _navigationQueryService = null!;
    private Guid _leafKey;

    // 3 is a typical site (root, section, page), 40 is deeper than the stack buffer used to collect the keys.
    [Params(3, 8, 40)]
    public int Depth { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var navigationService = new DocumentNavigationService(
            Mock.Of<ICoreScopeProvider>(),
            Mock.Of<INavigationRepository>(),
            Mock.Of<IContentTypeService>());

        var contentTypeKey = Guid.NewGuid();
        Guid? parentKey = null;
        for (var i = 0; i < Depth; i++)
        {
            var key = Guid.NewGuid();
            navigationService.Add(key, contentTypeKey, parentKey);
            parentKey = key;
        }

        _leafKey = parentKey!.Value;
        _navigationQueryService = navigationService;
    }

    [Benchmark]
    public int TryGetAncestorsKeys()
    {
        _navigationQueryService.TryGetAncestorsKeys(_leafKey, out IEnumerable<Guid> ancestorsKeys);
        return ancestorsKeys.Count();
    }

    [Benchmark]
    public int TryGetAncestorsOrSelfKeys()
    {
        _navigationQueryService.TryGetAncestorsOrSelfKeys(_leafKey, out IEnumerable<Guid> ancestorsOrSelfKeys);
        return ancestorsOrSelfKeys.Count();
    }
}
