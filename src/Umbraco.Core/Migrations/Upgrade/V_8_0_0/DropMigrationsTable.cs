namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropMigrationsTable : MigrationBase
    {
        public DropMigrationsTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (TableExists("umbracoMigration"))
                Delete.Table("umbracoMigration").Do();
        }
    }
}
