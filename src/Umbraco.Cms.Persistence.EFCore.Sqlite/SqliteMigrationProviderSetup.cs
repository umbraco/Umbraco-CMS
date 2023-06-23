using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

public class SqliteMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => "Microsoft.Data.Sqlite";
    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlite(connectionString, x=>x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
