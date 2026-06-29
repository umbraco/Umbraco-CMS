using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Represents a contract for executing migrations using Entity Framework Core within the Umbraco CMS infrastructure.
/// </summary>
public interface IEFCoreMigrationExecutor
{
    /// <summary>
    /// Asynchronously executes a single Entity Framework Core migration against the database.
    /// </summary>
    /// <param name="efCoreMigration">The EF Core migration instance to execute.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execution of the migration.</returns>
    Task ExecuteSingleMigrationAsync(EFCoreMigration efCoreMigration);

    /// <summary>
    /// Executes all pending Entity Framework Core migrations for the current database context asynchronously.
    /// </summary>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the asynchronous migration operation.</returns>
    Task ExecuteAllMigrationsAsync();
}
