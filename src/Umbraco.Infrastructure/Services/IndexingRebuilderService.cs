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

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
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
    [Obsolete("Use TryRebuildAsync instead. Scheduled for removal in Umbraco 19.")]
    public bool TryRebuild(IIndex index, string indexName)
        => TryRebuildAsync(index, indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public async Task<bool> TryRebuildAsync(IIndex index, string indexName)
    {
        try
        {
            Attempt<IndexRebuildResult> attempt = await _indexRebuilder.RebuildIndexAsync(indexName);
            return attempt.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred rebuilding index");
            return false;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use IsRebuildingAsync() instead. Scheduled for removal in Umbraco 19.")]
    public bool IsRebuilding(string indexName)
        => IsRebuildingAsync(indexName).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task<bool> IsRebuildingAsync(string indexName)
        => _indexRebuilder.IsRebuildingAsync(indexName);
}
