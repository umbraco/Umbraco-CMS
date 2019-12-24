namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropTemplateDesignColumn : MigrationBase
    {
        public DropTemplateDesignColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if(ColumnExists("cmsTemplate", "design"))
                Delete.Column("design").FromTable("cmsTemplate").Do();
        }
    }
}
