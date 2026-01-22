using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Configures the EF Core DbContext to use SQLite as the database provider.
/// </summary>
public class SqliteMigrationProviderSetup : IMigrationProviderSetup
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderNames.SQLLite;

    /// <inheritdoc />
    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlite(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
