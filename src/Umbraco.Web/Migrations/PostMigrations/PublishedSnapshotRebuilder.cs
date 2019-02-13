using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Migrations.PostMigrations
{
    /// <summary>
    /// Implements <see cref="IPublishedSnapshotRebuilder"/> in Umbraco.Web (rebuilding).
    /// </summary>
    public class PublishedSnapshotRebuilder : IPublishedSnapshotRebuilder
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedSnapshotRebuilder"/> class.
        /// </summary>
        /// <param name="publishedSnapshotService"></param>
        public PublishedSnapshotRebuilder(IPublishedSnapshotService publishedSnapshotService)
        {
            _publishedSnapshotService = publishedSnapshotService;
        }

        /// <inheritdoc />
        public void Rebuild()
        {
            _publishedSnapshotService.Rebuild();
        }
    }
}
