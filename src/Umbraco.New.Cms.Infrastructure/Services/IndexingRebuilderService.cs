using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.New.Cms.Infrastructure.Services;

public class IndexingRebuilderService : IIndexingRebuilderService
{
    private const string TempKey = "temp_indexing_op_";
    private readonly IAppPolicyCache _runtimeCache;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<IndexingRebuilderService> _logger;

    public IndexingRebuilderService(
        AppCaches runtimeCache,
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
    {
        _indexRebuilder = indexRebuilder;
        _logger = logger;
        _runtimeCache = runtimeCache.RuntimeCache;
    }

    public bool CanRebuild(string indexName) => _indexRebuilder.CanRebuild(indexName);

    public bool TryRebuild(IIndex index, string indexName)
    {
        // Remove it in case there's a handler there already
        index.IndexOperationComplete -= Indexer_IndexOperationComplete;

        // Now add a single handler
        index.IndexOperationComplete += Indexer_IndexOperationComplete;

        try
        {
            Set(indexName);
            _indexRebuilder.RebuildIndex(indexName);
            return true;
        }
        catch(Exception exception)
        {
            // Ensure it's not listening
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;
            _logger.LogError(exception, "An error occurred rebuilding index");
            return false;
        }
    }

    private void Set(string indexName)
    {
        var cacheKey = TempKey + indexName;

        // put temp val in cache which is used as a rudimentary way to know when the indexing is done
        _runtimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));
    }

    private void Clear(string? indexName)
    {
        var cacheKey = TempKey + indexName;
        _runtimeCache.Clear(cacheKey);
    }

    public bool IsRebuilding(string indexName)
    {
        var cacheKey = "temp_indexing_op_" + indexName;
        return _runtimeCache.Get(cacheKey) is not null;
    }

    private void Indexer_IndexOperationComplete(object? sender, EventArgs e)
    {
        var indexer = (IIndex?)sender;

        _logger.LogDebug("Logging operation completed for index {IndexName}", indexer?.Name);

        if (indexer is not null)
        {
            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
        }

        _logger.LogInformation($"Rebuilding index '{indexer?.Name}' done.");

        Clear(indexer?.Name);
    }
}
