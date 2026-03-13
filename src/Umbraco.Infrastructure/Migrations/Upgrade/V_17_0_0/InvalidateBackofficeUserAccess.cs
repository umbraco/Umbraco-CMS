namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Represents a migration step that invalidates backoffice user access during the upgrade process.
/// </summary>
public class InvalidateBackofficeUserAccess : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateBackofficeUserAccess"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public InvalidateBackofficeUserAccess(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        InvalidateBackofficeUserAccess = true;
        return Task.CompletedTask;
    }
}
