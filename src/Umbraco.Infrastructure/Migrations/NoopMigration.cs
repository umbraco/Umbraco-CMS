
namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Represents a migration that intentionally performs no actions when executed.
/// This can be used as a placeholder or for scenarios where a migration step is required but no changes are needed.
/// </summary>
public class NoopMigration : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoopMigration"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration.</param>
    public NoopMigration(IMigrationContext context)
        : base(context)
    { }

    protected override Task MigrateAsync()
        => Task.CompletedTask;
}
