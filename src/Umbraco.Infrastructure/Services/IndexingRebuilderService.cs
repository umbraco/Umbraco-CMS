using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Services;

/// <inheritdoc />
public class IndexingRebuilderService : IIndexingRebuilderService
{
    private const string IsRebuildingIndexRuntimeCacheKeyPrefix = "temp_indexing_op_";

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

    [Obsolete("Use the constructor that accepts AppCaches. Scheduled for removal in Umbraco 19.")]
    public IndexingRebuilderService(
        IIndexRebuilder indexRebuilder,
        ILogger<IndexingRebuilderService> logger)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<AppCaches>(),
            indexRebuilder,
            logger)
    {
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
            Set(indexName);
            Attempt<IndexRebuildResult> result = await _indexRebuilder.RebuildIndexAsync(indexName);

            if (result.Success is false)
            {
                Clear(indexName);
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Clear(indexName);
            _logger.LogError(exception, "An error occurred rebuilding index");
            return false;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use IsRebuildingAsync() instead. Scheduled for removal in Umbraco 19.")]
    public bool IsRebuilding(string indexName)
    {
        var cacheKey = IsRebuildingIndexRuntimeCacheKeyPrefix + indexName;
        return _runtimeCache.Get(cacheKey) is not null;
    }

    /// <inheritdoc />
    public Task<bool> IsRebuildingAsync(string indexName)
    {
        return Task.FromResult(IsRebuilding(indexName));
    }

    private void Set(string indexName)
    {
        var cacheKey = IsRebuildingIndexRuntimeCacheKeyPrefix + indexName;

        // put temp val in cache which is used as a rudimentary way to know when the indexing is done
        _runtimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));
    }

    private void Clear(string indexName)
    {
        var cacheKey = IsRebuildingIndexRuntimeCacheKeyPrefix + indexName;
        _runtimeCache.Clear(cacheKey);
    }
}
