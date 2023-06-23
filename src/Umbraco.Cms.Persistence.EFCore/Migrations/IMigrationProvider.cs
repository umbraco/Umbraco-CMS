namespace Umbraco.Cms.Persistence.EFCore.Migrations;

public interface IMigrationProvider
{
    string ProviderName { get; }
    Task MigrateAsync(EFCoreMigration migration);
}
