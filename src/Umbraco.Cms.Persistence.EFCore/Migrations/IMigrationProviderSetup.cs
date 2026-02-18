using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Persistence.EFCore.Migrations;

/// <summary>
/// Provides setup configuration for a database provider's DbContext options.
/// </summary>
public interface IMigrationProviderSetup
{
    /// <summary>
    /// Gets the name of the database provider this setup supports.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Configures the DbContext options builder for the specific database provider.
    /// </summary>
    /// <param name="builder">The DbContext options builder to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    void Setup(DbContextOptionsBuilder builder, string? connectionString);
}
