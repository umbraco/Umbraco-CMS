using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IEFCoreDatabaseCreator
{
    Task ExecuteSingleMigrationAsync(EFCoreMigration efCoreMigration);

    Task ExecuteAllMigrationsAsync();
}
