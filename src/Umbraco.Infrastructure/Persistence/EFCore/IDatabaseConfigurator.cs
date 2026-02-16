using Microsoft.EntityFrameworkCore;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// Configures the database provider on a <see cref="DbContextOptionsBuilder"/> without setting migration assemblies.
/// </summary>
/// <remarks>
/// Each database provider (e.g., SQLite, SQL Server) implements this interface to handle provider-specific
/// configuration. The <see cref="CanHandle"/> method allows the implementation to own provider-name matching logic.
/// </remarks>
public interface IDatabaseConfigurator
{
    /// <summary>
    /// Determines whether this configurator can handle the specified provider name.
    /// </summary>
    /// <param name="providerName">The database provider name from the connection string configuration.</param>
    /// <returns><c>true</c> if this configurator can handle the provider; otherwise, <c>false</c>.</returns>
    bool CanHandle(string providerName);

    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> for the specific database provider.
    /// </summary>
    /// <param name="builder">The DbContext options builder to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    void Configure(DbContextOptionsBuilder builder, string connectionString);
}
