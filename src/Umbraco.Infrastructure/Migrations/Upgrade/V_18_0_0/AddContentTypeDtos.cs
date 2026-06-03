using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddContentTypeDtos : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public AddContentTypeDtos(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync()
    {
        // NO-OP migration to ensure that EF Core doesn't complain about missing migrations.
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddContentTypeDtos);
    }
}
