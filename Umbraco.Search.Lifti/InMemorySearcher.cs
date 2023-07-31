using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.InMemory;

public class UmbracoMemorySearcher<T> : IUmbracoSearcher<T>
{
    private readonly IMemoryCache _memoryCache;

    public UmbracoMemorySearcher(IMemoryCache memoryCache, string name)
    {
        _memoryCache = memoryCache;
        Name = name;
    }

    public UmbracoSearchResults Search(string term, int page, int pageSize)
    {


    }
    public string Name { get; }

    public UmbracoSearchResults? NativeQuery(string query, int page, int pageSize)
    {
        if (query.Contains(":"))
        {

        }
    }

    public IEnumerable<PublishedSearchResult> SearchDescendants(IPublishedContent content, string term) => throw new NotImplementedException();

    public IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, string term) => throw new NotImplementedException();

    public IUmbracoSearchResults Search(ISearchRequest searchRequest) => throw new NotImplementedException();

    public ISearchRequest CreateSearchRequest() => throw new NotImplementedException();

    public IEnumerable<PublishedSearchResult> GetAll() => throw new NotImplementedException();
}
