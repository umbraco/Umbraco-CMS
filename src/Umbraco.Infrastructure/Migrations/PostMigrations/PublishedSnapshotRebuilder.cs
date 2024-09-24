using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Implements <see cref="IPublishedSnapshotRebuilder" /> in Umbraco.Web (rebuilding).
/// </summary>
public class PublishedSnapshotRebuilder : IPublishedSnapshotRebuilder
{
    private readonly DistributedCache _distributedCache;
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedSnapshotRebuilder" /> class.
    /// </summary>
    public PublishedSnapshotRebuilder(
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
