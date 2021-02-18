namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class TagsMigrationFix : MigrationBase
    {
        public TagsMigrationFix(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // kill unused parentId column, if it still exists
            if (ColumnExists(Cms.Core.Constants.DatabaseSchema.Tables.Tag, "ParentId"))
                Delete.Column("ParentId").FromTable(Cms.Core.Constants.DatabaseSchema.Tables.Tag).Do();
        }
    }
}
