using NUnit.Framework;
using Umbraco.Cms.Core;
using ISearcher = Umbraco.Cms.Search.Core.Services.ISearcher;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.SearchService;

public abstract class SearcherTestBase : TestBase
{
    protected ISearcher Searcher => GetRequiredService<ISearcher>();
}
