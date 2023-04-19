namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_12_0_0;

public class ResetCache : MigrationBase
{
    public ResetCache(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate() => RebuildCache = true;
}
