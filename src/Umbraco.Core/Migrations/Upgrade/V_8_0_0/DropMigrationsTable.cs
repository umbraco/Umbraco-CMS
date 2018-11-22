namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class DropMigrationsTable : MigrationBase
    {
        public DropMigrationsTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Delete.Table("umbracoMigration").Do();
        }
    }
}
