using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

public class SqliteMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => Constants.ProviderNames.SQLLite;

    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlite(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
