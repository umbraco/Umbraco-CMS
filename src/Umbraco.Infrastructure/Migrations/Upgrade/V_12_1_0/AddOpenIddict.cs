namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_1_0;

public class AddOpenIddict : UnscopedMigrationBase
{
    private readonly IOpenIddictDatabaseCreator _openIddictDatabaseCreator;

    public AddOpenIddict(IMigrationContext context, IOpenIddictDatabaseCreator openIddictDatabaseCreator)
        : base(context)
    {
        _openIddictDatabaseCreator = openIddictDatabaseCreator;
    }

    protected override void Migrate()
    {
        _openIddictDatabaseCreator.CreateAsync().GetAwaiter().GetResult();
    }
}

