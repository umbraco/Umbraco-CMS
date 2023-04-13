namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IMigrationService
{
    Task MigrateAsync(string migrationName);
}
