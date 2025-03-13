using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;
using Umbraco.Cms.Persistence.EFCore;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

public class SqliteMigrationProvider : IMigrationProvider
{
    private readonly IDbContextFactory<UmbracoDbContext> _dbContextFactory;

    public SqliteMigrationProvider(IDbContextFactory<UmbracoDbContext> dbContextFactory)
        => _dbContextFactory = dbContextFactory;

    public string ProviderName => Constants.ProviderNames.SQLLite;

    public async Task MigrateAsync(EFCoreMigration migration)
    {
        UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.MigrateDatabaseAsync(GetMigrationType(migration));
    }

    public async Task MigrateAllAsync()
    {
        UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();

        if (context.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException("Cannot migrate all when a transaction is active.");
        }

        await context.Database.MigrateAsync();
    }

    private static Type GetMigrationType(EFCoreMigration migration) =>
        migration switch
        {
            EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
            EFCoreMigration.AddOpenIddict => typeof(Migrations.AddOpenIddict),
            _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
        };
}
