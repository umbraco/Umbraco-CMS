using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Implements <see cref="ICacheRebuilder" /> in Umbraco.Web (rebuilding).
/// </summary>
public class CacheRebuilder : ICacheRebuilder
{
    private readonly DistributedCache _distributedCache;
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRebuilder" /> class.
    /// </summary>
    public CacheRebuilder(
        DistributedCache distributedCache,
        IDatabaseCacheRebuilder databaseCacheRebuilder)
    {
        _distributedCache = distributedCache;
        _databaseCacheRebuilder = databaseCacheRebuilder;
    }

    /// <inheritdoc />
    public void Rebuild()
    {
        _databaseCacheRebuilder.Rebuild();
        _distributedCache.RefreshAllPublishedSnapshot();
    }
}
