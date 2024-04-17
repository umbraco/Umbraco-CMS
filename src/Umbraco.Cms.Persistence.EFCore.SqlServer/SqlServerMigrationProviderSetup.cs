using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

public class SqlServerMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => Constants.ProviderNames.SQLServer;

    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlServer(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
