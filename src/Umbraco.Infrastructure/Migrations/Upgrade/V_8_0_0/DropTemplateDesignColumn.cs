namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

public class DropTemplateDesignColumn : MigrationBase
{
    public DropTemplateDesignColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (ColumnExists("cmsTemplate", "design"))
        {
            Delete.Column("design").FromTable("cmsTemplate").Do();
        }
    }
}
