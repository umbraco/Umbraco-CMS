using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Search.Indexing;

<<<<<<<< HEAD:src/Umbraco.Search/Services/IndexingRebuilderService.cs
namespace Umbraco.Search.Services;
========
namespace Umbraco.Cms.Infrastructure.Services;
>>>>>>>> origin/v13/dev:src/Umbraco.Infrastructure/Services/IndexingRebuilderService.cs

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

    public bool TryRebuild(string indexName)
    {
        try
        {
            Set(indexName);
            _indexRebuilder.RebuildIndex(indexName);
            return true;
        }
        catch(Exception exception)
        {
            // Ensure it's not listening
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
}
