namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
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
