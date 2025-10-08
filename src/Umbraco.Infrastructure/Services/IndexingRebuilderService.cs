using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Services;

/// <inheritdoc />
public class IndexingRebuilderService : IIndexingRebuilderService
{
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<IndexingRebuilderService> _logger;

    public IndexingRebuilderService(
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
    {
        _indexRebuilder = indexRebuilder;
        _logger = logger;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
    public IndexingRebuilderService(
        AppCaches runtimeCache,
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
    {
        _indexRebuilder = indexRebuilder;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool CanRebuild(string indexName) => _indexRebuilder.CanRebuild(indexName);

    /// <inheritdoc />
    [Obsolete("Use TryRebuildAsync instead. Scheduled for removal in V19.")]
    public bool TryRebuild(IIndex index, string indexName)
        => TryRebuildAsync(index, indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<bool> TryRebuildAsync(IIndex index, string indexName)
    {
        // Remove it in case there's a handler there already
        index.IndexOperationComplete -= Indexer_IndexOperationComplete;

        // Now add a single handler
        index.IndexOperationComplete += Indexer_IndexOperationComplete;

        try
        {
            Attempt<IndexRebuildResult> attempt = await _indexRebuilder.RebuildIndexAsync(indexName);
            return attempt.Success;
        }
        catch (Exception exception)
        {
            // Ensure it's not listening
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;
            _logger.LogError(exception, "An error occurred rebuilding index");
            return false;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use IsRebuildingAsync() instead. Scheduled for removal in V19.")]
    public bool IsRebuilding(string indexName)
        => IsRebuildingAsync(indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<bool> IsRebuildingAsync(string indexName)
        => _indexRebuilder.IsRebuildingAsync(indexName);

    private void Indexer_IndexOperationComplete(object? sender, EventArgs e)
    {
        var indexer = (IIndex?)sender;

        _logger.LogDebug("Logging operation completed for index {IndexName}", indexer?.Name);

        if (indexer is not null)
        {
            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
        }

        _logger.LogInformation("Rebuilding index '{IndexerName}' done.", indexer?.Name);
    }
}
