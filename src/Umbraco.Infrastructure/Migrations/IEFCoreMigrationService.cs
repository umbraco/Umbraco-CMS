namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IEFCoreMigrationService
{
    Task MigrateAsync();

    Task<string?> GetCurrentMigrationStateAsync();

    Task<string> GetFinalMigrationStateAsync();
}
