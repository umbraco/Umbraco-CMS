using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

public class UpdateToOpenIddictV7 : MigrationBase
{
    private readonly IEFCoreMigrationExecutor _efCoreMigrationExecutor;

    public UpdateToOpenIddictV7(IMigrationContext context, IEFCoreMigrationExecutor efCoreMigrationExecutor)
        : base(context)
    {
        _efCoreMigrationExecutor = efCoreMigrationExecutor;
    }

    protected override void Migrate()
    {
        _efCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.UpdateOpenIddictToV7);
    }
}
