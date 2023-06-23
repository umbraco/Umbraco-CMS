using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.OpenIddict;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

public class SqliteMigrationProvider : IMigrationProvider
{
    private readonly IDbContextFactory<UmbracoOpenIddictDbContext> _dbContextFactory;

    public SqliteMigrationProvider(IDbContextFactory<UmbracoOpenIddictDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public string ProviderName => "Microsoft.Data.Sqlite";

    public async Task MigrateAsync(EFCoreMigration migration)
    {
        UmbracoOpenIddictDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.MigrateDatabaseAsync(GetMigrationType(migration));
    }

    public async Task MigrateAll()
    {
        UmbracoOpenIddictDbContext context = await _dbContextFactory.CreateDbContextAsync();

        if (context.Database.CurrentTransaction is not null)
        {
            //SUPER HACK if we are in trasaction we need to commit it and start a new.
            context.Database.CommitTransaction();


            await context.Database.MigrateAsync();

            context.Database.BeginTransaction();
        }
        else
        {
            await context.Database.MigrateAsync();
        }


    }

    private static Type GetMigrationType(EFCoreMigration migration) =>
        migration switch
        {
            EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
            _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
        };
}
