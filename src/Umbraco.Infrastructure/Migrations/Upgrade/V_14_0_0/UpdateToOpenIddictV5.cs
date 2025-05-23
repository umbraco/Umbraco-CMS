using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class UpdateToOpenIddictV5 : MigrationBase
{
    private readonly IEFCoreMigrationExecutor _efCoreMigrationExecutor;

    public UpdateToOpenIddictV5(IMigrationContext context, IEFCoreMigrationExecutor efCoreMigrationExecutor)
        : base(context)
    {
        _efCoreMigrationExecutor = efCoreMigrationExecutor;
    }

    protected override void Migrate()
    {
        _efCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.UpdateOpenIddictToV5);
    }
}
