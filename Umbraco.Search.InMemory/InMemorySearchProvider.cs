using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Search.InMemory;

public class InMemorySearchProvider : ISearchProvider
{
    private readonly IInMemoryIndexManager _inMemoryIndexManager;
    private readonly ILogger<InMemorySearchProvider> _logger;

    public InMemorySearchProvider(IInMemoryIndexManager inMemoryIndexManager, ILogger<InMemorySearchProvider> logger)
    {
        _inMemoryIndexManager = inMemoryIndexManager;
        _logger = logger;
    }

    public IUmbracoIndex? GetIndex(string index)
    {
        return _inMemoryIndexManager.GetIndex(index);
    }

    public IUmbracoIndex<T>? GetIndex<T>(string index) where T : IUmbracoEntity  {
        return _inMemoryIndexManager.GetIndex<T>(index);
    }

    public IUmbracoSearcher? GetSearcher(string index) => throw new NotImplementedException();

    public IEnumerable<string> GetAllIndexes() => throw new NotImplementedException();

    public IEnumerable<string> GetUnhealthyIndexes() => throw new NotImplementedException();

    public OperationResult CreateIndex(string indexName) => throw new NotImplementedException();

    public IEnumerable<string> GetAllSearchers() => throw new NotImplementedException();
}
