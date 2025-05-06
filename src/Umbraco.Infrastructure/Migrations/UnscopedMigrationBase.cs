namespace Umbraco.Cms.Infrastructure.Migrations;

/// <inheritdoc />
public abstract class UnscopedMigrationBase : UnscopedAsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnscopedMigrationBase" /> class.
    /// </summary>
    /// <param name="context">The context.</param>
    protected UnscopedMigrationBase(IMigrationContext context)
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
