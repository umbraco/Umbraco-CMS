namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropPreValueTable : MigrationBase
    {
        public DropPreValueTable(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            // drop preValues table
            if (TableExists("cmsDataTypePreValues"))
                Delete.Table("cmsDataTypePreValues").Do();
        }
    }
}
