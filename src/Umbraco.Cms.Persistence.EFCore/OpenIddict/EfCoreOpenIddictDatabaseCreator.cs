using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.OpenIddict;

public class EfCoreOpenIddictDatabaseCreator : IOpenIddictDatabaseCreator
{
    private readonly IEnumerable<IMigrationProvider> _migrationProviders;
    private readonly IDbContextFactory<UmbracoOpenIddictDbContext> _dbContextFactory;
    private readonly IOptions<ConnectionStrings> _options;

    // We need to do migrations out side of a scope due to sqlite
    public EfCoreOpenIddictDatabaseCreator(
        IEnumerable<IMigrationProvider> migrationProviders,
        IDbContextFactory<UmbracoOpenIddictDbContext> dbContextFactory,
        IOptions<ConnectionStrings> options)
    {
        _migrationProviders = migrationProviders;
        _dbContextFactory = dbContextFactory;
        _options = options;
    }

    public async Task ExecuteSingleMigrationAsync(EFCoreMigration migration)
    {
        var provider = _migrationProviders.FirstOrDefault(x => x.ProviderName == _options.Value.ProviderName);

        if (provider is not null)
        {
            await provider.MigrateAsync(migration);
        }
    }

    public async Task ExecuteAllMigrationsAsync()
    {
        var provider = _migrationProviders.FirstOrDefault(x => x.ProviderName == _options.Value.ProviderName);

        if (provider is not null)
        {
            await provider.MigrateAll();
        }
    }

}
