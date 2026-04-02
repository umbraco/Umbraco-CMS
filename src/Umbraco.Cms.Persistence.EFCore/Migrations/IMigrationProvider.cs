namespace Umbraco.Cms.Persistence.EFCore.Migrations;

/// <summary>
/// Provides database migration capabilities for a specific database provider.
/// </summary>
public interface IMigrationProvider
{
    /// <summary>
    /// Gets the name of the database provider this migration provider supports.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Executes a specific EF Core migration.
    /// </summary>
    /// <param name="migration">The migration to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MigrateAsync(EFCoreMigration migration);

    /// <summary>
    /// Executes all pending EF Core migrations.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MigrateAllAsync();
}
