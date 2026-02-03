using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Constants = Umbraco.Cms.Infrastructure.Persistence.EFCore.Constants;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// Configures the EF Core DbContext to use SQL Server as the database provider.
/// </summary>
public class SqlServerMigrationProviderSetup : IMigrationProviderSetup
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderNames.SQLServer;

    /// <inheritdoc />
    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlServer(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
