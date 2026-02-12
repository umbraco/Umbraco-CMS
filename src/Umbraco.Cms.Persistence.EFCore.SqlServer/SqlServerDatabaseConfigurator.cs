using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Constants = Umbraco.Cms.Infrastructure.Persistence.EFCore.Constants;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// Configures the <see cref="DbContextOptionsBuilder"/> to use SQL Server as the database provider.
/// </summary>
public class SqlServerDatabaseConfigurator : IDatabaseConfigurator
{
    /// <inheritdoc />
    public bool CanHandle(string providerName)
        => string.Equals(providerName, Constants.ProviderNames.SQLServer, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Configure(DbContextOptionsBuilder builder, string connectionString)
        => builder.UseSqlServer(connectionString);
}
