using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.Lifti;

public class UmbracoMemorySearcher<T> : IUmbracoSearcher<T>
{
    private readonly ILiftiIndex? _liftiIndex;

    public UmbracoMemorySearcher(ILiftiIndex? liftiIndex, string name)
    {
        _liftiIndex = liftiIndex;
        Name = name;
    }

    public UmbracoSearchResults Search(string term, int page, int pageSize)
    {
        var allResult = _liftiIndex?.LiftiIndex.Search(term);
        if (allResult != null)
        {
            var results =allResult.Skip(pageSize * page).Take(pageSize).ToUmbracoResults();
            return new UmbracoSearchResults(allResult.Count(), pageSize,results);
        }
        return new UmbracoSearchResults(0, pageSize,new List<IUmbracoSearchResult>());
    }
    public string Name { get; }

    public UmbracoSearchResults? NativeQuery(string query, int page, int pageSize)
    {
        var allResult = _liftiIndex?.LiftiIndex.Search(query);
        if (allResult != null)
        {
            var results =allResult.Skip(pageSize * page).Take(pageSize).ToUmbracoResults();
            return new UmbracoSearchResults(allResult.Count(), pageSize,results);
        }
        return new UmbracoSearchResults(0, pageSize,new List<IUmbracoSearchResult>());
    }

    public IEnumerable<PublishedSearchResult> SearchDescendants(IPublishedContent content, string term) => throw new NotImplementedException();

    public IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, string term) => throw new NotImplementedException();

    public IUmbracoSearchResults Search(ISearchRequest searchRequest) => throw new NotImplementedException();

    public ISearchRequest CreateSearchRequest() => throw new NotImplementedException();

    public IEnumerable<PublishedSearchResult> GetAll() => throw new NotImplementedException();
}
