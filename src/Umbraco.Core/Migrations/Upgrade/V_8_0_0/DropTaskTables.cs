namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropTaskTables : MigrationBase
    {
        public DropTaskTables(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Delete.Table("cmsTaskType");
            Delete.Table("cmsTask");
        }
    }
}
