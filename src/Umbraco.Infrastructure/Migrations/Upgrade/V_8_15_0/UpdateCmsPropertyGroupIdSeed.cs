namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_15_0;

public class UpdateCmsPropertyGroupIdSeed : MigrationBase
{
    public UpdateCmsPropertyGroupIdSeed(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // NOOP - was sql ce only
    }
}
