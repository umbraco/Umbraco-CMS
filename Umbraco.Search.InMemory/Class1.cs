using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Search.InMemory;

public class InMemorySearchProvider : ISearchProvider
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemorySearchProvider> _logger;

    public InMemorySearchProvider(IMemoryCache memoryCache, ILogger<InMemorySearchProvider> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public IUmbracoIndex? GetIndex(string index)
    {
return new UmbracoMemoryIndex<>()
    }

    public IUmbracoIndex<T>? GetIndex<T>(string index) => throw new NotImplementedException();

    public IUmbracoSearcher? GetSearcher(string index) => throw new NotImplementedException();

    public IEnumerable<string> GetAllIndexes() => throw new NotImplementedException();

    public IEnumerable<string> GetUnhealthyIndexes() => throw new NotImplementedException();

    public OperationResult CreateIndex(string indexName) => throw new NotImplementedException();

    public IEnumerable<string> GetAllSearchers() => throw new NotImplementedException();
}
