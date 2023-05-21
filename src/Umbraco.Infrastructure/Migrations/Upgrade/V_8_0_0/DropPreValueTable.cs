namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class DropPreValueTable : MigrationBase
{
    public DropPreValueTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // drop preValues table
        if (TableExists("cmsDataTypePreValues"))
        {
            Delete.Table("cmsDataTypePreValues").Do();
        }
    }
}
