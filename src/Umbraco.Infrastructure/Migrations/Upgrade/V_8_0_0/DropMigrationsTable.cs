namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

[Obsolete("This is not used anymore and will be removed in Umbraco 13")]
public class DropMigrationsTable : MigrationBase
{
    public DropMigrationsTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (TableExists("umbracoMigration"))
        {
            Delete.Table("umbracoMigration").Do();
        }
    }
}
