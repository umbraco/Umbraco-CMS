using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Constants = Umbraco.Cms.Infrastructure.Persistence.EFCore.Constants;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Configures the <see cref="DbContextOptionsBuilder"/> to use SQLite as the database provider.
/// </summary>
public class SqliteDatabaseConfigurator : IDatabaseConfigurator
{
    /// <inheritdoc />
    public bool CanHandle(string providerName)
        => string.Equals(providerName, Constants.ProviderNames.SQLLite, StringComparison.OrdinalIgnoreCase)
        || string.Equals(providerName, "Microsoft.Data.SQLite", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Configure(DbContextOptionsBuilder builder, string connectionString)
        => builder.UseSqlite(connectionString);
}
