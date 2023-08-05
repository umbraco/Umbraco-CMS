using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Search.Diagnostics;

namespace Umbraco.Search.Lifti;

public class InMemorySearchProvider : ISearchProvider
{
    public IEnumerable<IUmbracoIndex> Indexes => _indexers;

    private readonly ILogger<InMemorySearchProvider> _logger;
    private readonly IEnumerable<IUmbracoIndex> _indexers;
    private IEnumerable<IUmbracoSearcher> _searchers;

    //todo: make valuesetbuilders easier to manage
    public InMemorySearchProvider(ILogger<InMemorySearchProvider> logger, IEnumerable<IUmbracoIndex> indexes, IEnumerable<IUmbracoSearcher> searchers)
    {
        _logger = logger;
        _indexers = indexes;
        _searchers = searchers;
    }

    public IUmbracoIndex? GetIndex(string index)
    {
        return _indexers.FirstOrDefault(x => x.Name == index);
    }

    public IUmbracoIndex<T>? GetIndex<T>(string index) where T : IUmbracoEntity
    {
        return GetIndex(index) as IUmbracoIndex<T>;
    }

    public IUmbracoSearcher? GetSearcher(string index) => _searchers.FirstOrDefault(x => x.Name == index);

    public IUmbracoSearcher<T>? GetSearcher<T>(string index)
    {
        return GetSearcher(index) as IUmbracoSearcher<T>;
    }

    public IEnumerable<string> GetAllIndexes()
    {
        return _indexers.Select(x => x.Name);
    }

    public IEnumerable<string> GetUnhealthyIndexes()
    {
        return _indexers.Where(x => !x.Exists() || ( x is IIndexDiagnostics indexProvider &&  !indexProvider.IsHealthy())).Select(x=>x.Name);
    }

    public OperationResult CreateIndex(string indexName)
    {
        var messages = new EventMessages();

        var index = _indexers.FirstOrDefault(x=>x.Name==indexName);
        if (index == null)
        {
            messages.Add(new EventMessage("Lifti Search provider", "Lifti has no registration for index", EventMessageType.Error));
            return OperationResult.Cancel(messages);
        }

        try
        {
            index.Create();
            return OperationResult.Succeed(messages);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Creating Index {name} failed",indexName);
            messages.Add(new EventMessage("Examine Search provider", "Examine failed to created index", EventMessageType.Error));
            return OperationResult.Cancel(messages);
        }
    }

    public IEnumerable<string> GetAllSearchers()
    {
        return _searchers.Select(x => x.Name);
    }
}
