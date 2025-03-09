namespace Umbraco.Cms.Infrastructure.Migrations;

/// <inheritdoc />
[Obsolete("Use AsyncMigrationBase instead. This class will be removed in a future version.")]
public abstract class MigrationBase : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationBase"/> class.
    /// </summary>
    /// <param name="context">A migration context.</param>
    protected MigrationBase(IMigrationContext context)
        : base(context)
    { }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        Migrate();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the migration.
    /// </summary>
    protected abstract void Migrate();
}
