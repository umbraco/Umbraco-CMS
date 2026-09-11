using ISearcher = Umbraco.Cms.Core.Search.ISearcher;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Cms.Search.Provider.Examine.Tests.ContentTests.SearchService;

public abstract class SearcherTestBase : TestBase
{
    protected ISearcher Searcher => GetRequiredService<ISearcher>();
}
