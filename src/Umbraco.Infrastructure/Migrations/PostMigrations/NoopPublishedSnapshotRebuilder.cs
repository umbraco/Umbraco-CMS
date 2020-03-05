namespace Umbraco.Core.Migrations.PostMigrations
{
    /// <summary>
    /// Implements <see cref="IPublishedSnapshotRebuilder"/> in Umbraco.Core (doing nothing).
    /// </summary>
    public class NoopPublishedSnapshotRebuilder : IPublishedSnapshotRebuilder
    {
        /// <inheritdoc />
        public void Rebuild()
        { }
    }
}
