
namespace Umbraco.Cms.Infrastructure.Migrations;

public class NoopMigration : AsyncMigrationBase
{
    public NoopMigration(IMigrationContext context)
        : base(context)
    { }

    protected override Task MigrateAsync()
        => Task.CompletedTask;
}
