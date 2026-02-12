namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class InvalidateBackofficeUserAccess : AsyncMigrationBase
{
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
