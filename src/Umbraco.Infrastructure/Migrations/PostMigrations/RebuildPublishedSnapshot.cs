using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations
{
    /// <summary>
    /// Rebuilds the published snapshot.
    /// </summary>
    public class RebuildPublishedSnapshot : MigrationBase
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly DistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RebuildPublishedSnapshot"/> class.
        /// </summary>
        public RebuildPublishedSnapshot(IMigrationContext context, IPublishedSnapshotService publishedSnapshotService, DistributedCache distributedCache)
            : base(context)
        {
            _publishedSnapshotService = publishedSnapshotService;
            _distributedCache = distributedCache;
        }

        /// <inheritdoc />
        protected override void Migrate()
        {
            _publishedSnapshotService.Rebuild();
            _distributedCache.RefreshAllPublishedSnapshot();
        }
    }
}
