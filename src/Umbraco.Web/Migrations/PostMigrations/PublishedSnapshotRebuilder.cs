using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Web.Cache;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Migrations.PostMigrations
{
    /// <summary>
    /// Implements <see cref="IPublishedSnapshotRebuilder"/> in Umbraco.Web (rebuilding).
    /// </summary>
    public class PublishedSnapshotRebuilder : IPublishedSnapshotRebuilder
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly DistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotRebuilder"/> class.
        /// </summary>
        /// <param name="publishedSnapshotService"></param>
        public PublishedSnapshotRebuilder(IPublishedSnapshotService publishedSnapshotService, DistributedCache distributedCache)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _distributedCache = distributedCache;
        }

        /// <inheritdoc />
        public void Rebuild()
        {
            _publishedSnapshotService.Rebuild();
            _distributedCache.RefreshAllPublishedSnapshot();
        }
    }
}
