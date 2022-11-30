namespace Umbraco.Cms.Infrastructure.Migrations.PostMigrations;

/// <summary>
///     Rebuilds the published snapshot.
/// </summary>
public class RebuildPublishedSnapshot : MigrationBase
{
    private readonly IPublishedSnapshotRebuilder _rebuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RebuildPublishedSnapshot" /> class.
    /// </summary>
    public RebuildPublishedSnapshot(IMigrationContext context, IPublishedSnapshotRebuilder rebuilder)
        : base(context)
        => _rebuilder = rebuilder;

    /// <inheritdoc />
    protected override void Migrate() => _rebuilder.Rebuild();
}
