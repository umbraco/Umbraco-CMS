namespace Umbraco.Core.Migrations.PostMigrations
{
    /// <summary>
    /// Rebuilds the published snapshot.
    /// </summary>
    public class RebuildPublishedSnapshot : IMigration
    {
        private readonly IPublishedSnapshotRebuilder _rebuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RebuildPublishedSnapshot"/> class.
        /// </summary>
        public RebuildPublishedSnapshot(IPublishedSnapshotRebuilder rebuilder)
        {
            _rebuilder = rebuilder;
        }

        /// <inheritdoc />
        public void Migrate()
        {
            _rebuilder.Rebuild();
        }
    }
}
