using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;

public class AddSqliteCollation : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public AddSqliteCollation(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor) : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync()
        => await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.SqliteCollation);
}
