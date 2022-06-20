namespace Umbraco.Cms.Infrastructure.Migrations;

public class NoopMigration : MigrationBase
{
    public NoopMigration(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // nop
    }
}
