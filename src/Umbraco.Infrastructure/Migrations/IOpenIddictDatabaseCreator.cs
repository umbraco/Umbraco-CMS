using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IOpenIddictDatabaseCreator
{
    Task ExecuteSingleMigrationAsync(EFCoreMigration efCoreMigration);

    Task ExecuteAllMigrationsAsync();
}
