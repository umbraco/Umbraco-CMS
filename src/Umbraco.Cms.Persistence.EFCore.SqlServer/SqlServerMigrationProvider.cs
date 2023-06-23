using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.OpenIddict;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

public class SqlServerMigrationProvider : IMigrationProvider
{
    private readonly IDbContextFactory<UmbracoOpenIddictDbContext> _dbContextFactory;
    public SqlServerMigrationProvider(IDbContextFactory<UmbracoOpenIddictDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public string ProviderName => "Microsoft.Data.SqlClient";
    public async Task MigrateAsync(EFCoreMigration migration)
    {
        UmbracoOpenIddictDbContext context = await _dbContextFactory.CreateDbContextAsync();
        await context.MigrateDatabaseAsync(GetMigrationType(migration));
    }

    private static Type GetMigrationType(EFCoreMigration migration) =>
        migration switch
        {
            EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
            _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
        };
}
