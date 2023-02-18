using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.ValueSetBuilders;

namespace Umbraco.Search.Examine;

public class ExamineSearchProvider : ISearchProvider
{
    private readonly IExamineManager _examineManager;
    private readonly IValueSetBuilderFactory _factory;

    private readonly ILogger<ExamineSearchProvider> _logger;

    //todo: make valuesetbuilders easier to manage
    public ExamineSearchProvider(IExamineManager examineManager, IValueSetBuilderFactory factory, ILogger<ExamineSearchProvider> logger)
    {
        _examineManager = examineManager;
        _factory = factory;
        _logger = logger;
    }

    public IUmbracoIndex<T> GetIndex<T>(string index)
    {
        var examineIndex = _examineManager.GetIndex(index);
        return new UmbracoExamineIndex<T>(examineIndex, _factory.Retrieve<T>());
    }

    public IUmbracoSearcher<T> GetSearcher<T>(string index)
    {
        var examineIndex = _examineManager.GetIndex(index).Searcher;
        return new UmbracoExamineSearcher<T>(examineIndex);
    }

    public IEnumerable<string> GetAllIndexes()
    {
        return _examineManager.Indexes.Select(x => x.Name);
    }

    public IEnumerable<string> GetUnhealthyIndexes()
    {
        return _examineManager.Indexes.Where(x => !x.IndexExists() || ( x is IIndexDiagnostics indexProvider &&  !indexProvider.IsHealthy())).Select(x=>x.Name);
    }

    public OperationResult CreateIndex(string indexName, Type indexValue)
    {
        var messages = new EventMessages();

        var getIndex = _examineManager.TryGetIndex(indexName, out var index);
        if (!getIndex)
        {
            messages.Add(new EventMessage("Examine Search provider", "Examine has no registration for index", EventMessageType.Error));
            return OperationResult.Cancel(messages);
        }

        try
        {
            index.CreateIndex();
            return OperationResult.Succeed(messages);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Creating Index {name} failed",indexName);
            messages.Add(new EventMessage("Examine Search provider", "Examine failed to created index", EventMessageType.Error));
            return OperationResult.Cancel(messages);
        }
    }
}
