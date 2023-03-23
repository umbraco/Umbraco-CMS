namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class AddEfCoreHistoryTable : EfCoreMigrationBase
{
    private readonly IEFCoreMigrationService _efCoreMigrationService;

    public AddEfCoreHistoryTable(IEFCoreMigrationContext context, IEFCoreMigrationService efCoreMigrationService) : base(context) => _efCoreMigrationService = efCoreMigrationService;

    protected override void Migrate() => _efCoreMigrationService.MigrateAsync().GetAwaiter().GetResult();
}
