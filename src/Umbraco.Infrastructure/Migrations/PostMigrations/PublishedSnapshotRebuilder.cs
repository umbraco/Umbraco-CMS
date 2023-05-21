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
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedSnapshotRebuilder" /> class.
    /// </summary>
    public PublishedSnapshotRebuilder(
        IPublishedSnapshotService publishedSnapshotService,
        DistributedCache distributedCache)
    {
        _publishedSnapshotService = publishedSnapshotService;
        _distributedCache = distributedCache;
    }

    /// <inheritdoc />
    public void Rebuild()
    {
        _publishedSnapshotService.RebuildAll();
        _distributedCache.RefreshAllPublishedSnapshot();
    }
}
