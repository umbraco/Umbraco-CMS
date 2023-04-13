namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class AddEFCoreHistoryTable : UnscopedMigrationBase
{
    private readonly IMigrationService _migrationService;

    public AddEFCoreHistoryTable(IMigrationContext context, IMigrationService migrationService) : base(context)
    {
        _migrationService = migrationService;
    }

    protected override void Migrate() => _migrationService.MigrateAsync("20220823084205_InitialState");
}
