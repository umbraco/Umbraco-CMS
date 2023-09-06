using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_1_0;

public class AddOpenIddict : UnscopedMigrationBase
{
    private readonly IEFCoreMigrationExecutor _iefCoreMigrationExecutor;

    public AddOpenIddict(IMigrationContext context, IEFCoreMigrationExecutor iefCoreMigrationExecutor)
        : base(context)
    {
        _iefCoreMigrationExecutor = iefCoreMigrationExecutor;
    }

    protected override void Migrate()
    {
        _iefCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.InitialCreate).GetAwaiter().GetResult();
        _iefCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddOpenIddict).GetAwaiter().GetResult();
    }
}

