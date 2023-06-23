using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_1_0;

public class AddOpenIddict : UnscopedMigrationBase
{
    private readonly IEFCoreDatabaseCreator _iefCoreDatabaseCreator;

    public AddOpenIddict(IMigrationContext context, IEFCoreDatabaseCreator iefCoreDatabaseCreator)
        : base(context)
    {
        _iefCoreDatabaseCreator = iefCoreDatabaseCreator;
    }

    protected override void Migrate()
    {
        _iefCoreDatabaseCreator.ExecuteSingleMigrationAsync(EFCoreMigration.InitialCreate).GetAwaiter().GetResult();
    }
}

