namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class DropXmlTables : MigrationBase
{
    public DropXmlTables(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists("cmsContentXml"))
        {
            Delete.Table("cmsContentXml").Do();
        }

        if (TableExists("cmsPreviewXml"))
        {
            Delete.Table("cmsPreviewXml").Do();
        }
    }
}
