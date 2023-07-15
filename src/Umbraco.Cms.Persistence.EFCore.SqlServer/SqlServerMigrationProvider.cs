using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

public class SqlServerMigrationProvider : IMigrationProvider
{
    private readonly IDbContextFactory<UmbracoInternalDbContext> _dbContextFactory;

    public SqlServerMigrationProvider(IDbContextFactory<UmbracoInternalDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

    public string ProviderName => "Microsoft.Data.SqlClient";

    public async Task MigrateAsync(EFCoreMigration migration)
    {
        UmbracoInternalDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.MigrateDatabaseAsync(GetMigrationType(migration));
    }

    public async Task MigrateAllAsync()
    {
        UmbracoInternalDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.Database.MigrateAsync();
    }

    private static Type GetMigrationType(EFCoreMigration migration) =>
        migration switch
        {
            EFCoreMigration.AddOpenIddict => typeof(Migrations.AddOpenIddict),
            _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
        };
}
