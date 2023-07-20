using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

public class SqlServerMigrationProviderSetup : IMigrationProviderSetup
{
    public string ProviderName => "Microsoft.Data.SqlClient";

    public void Setup(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseSqlServer(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
