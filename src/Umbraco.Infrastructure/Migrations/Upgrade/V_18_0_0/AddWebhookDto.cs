using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddWebhookDto : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public AddWebhookDto(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor) : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync()
    {
        // This is a No-OP migration that exists solely to ensure that the EFMigrationHistory is in sync with our migration classes.
        // To ensure that if we need a legitimate migration in the future EFCore doesn't complain about missing migrations.
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddWebhookDto);
    }
}
