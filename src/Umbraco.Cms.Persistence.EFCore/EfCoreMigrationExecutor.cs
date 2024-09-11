using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

public class EfCoreMigrationExecutor : IEFCoreMigrationExecutor
{
    private readonly IEnumerable<IMigrationProvider> _migrationProviders;
    private readonly IOptions<ConnectionStrings> _options;

    // We need to do migrations out side of a scope due to sqlite
    public EfCoreMigrationExecutor(
        IEnumerable<IMigrationProvider> migrationProviders,
        IOptions<ConnectionStrings> options)
    {
        _migrationProviders = migrationProviders;
        _options = options;
    }

    public async Task ExecuteSingleMigrationAsync(EFCoreMigration migration)
    {
        IMigrationProvider? provider = _migrationProviders.FirstOrDefault(x => x.ProviderName.CompareProviderNames(_options.Value.ProviderName));

        if (provider is not null)
        {
            await provider.MigrateAsync(migration);
        }
    }

    public async Task ExecuteAllMigrationsAsync()
    {
        IMigrationProvider? provider = _migrationProviders.FirstOrDefault(x => x.ProviderName.CompareProviderNames(_options.Value.ProviderName));
        if (provider is not null)
        {
            await provider.MigrateAllAsync();
        }
    }

}
