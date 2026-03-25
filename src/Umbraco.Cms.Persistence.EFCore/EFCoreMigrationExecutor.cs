using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

/// <summary>
/// Executes Entity Framework Core migrations for Umbraco.
/// </summary>
public class EFCoreMigrationExecutor : IEFCoreMigrationExecutor
{
    private readonly IEnumerable<IMigrationProvider> _migrationProviders;
    private readonly IOptions<ConnectionStrings> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreMigrationExecutor"/> class.
    /// </summary>
    /// <param name="migrationProviders">The collection of migration providers.</param>
    /// <param name="options">The connection string options.</param>
    /// <remarks>We need to do migrations outside of a scope due to SQLite.</remarks>
    public EFCoreMigrationExecutor(
        IEnumerable<IMigrationProvider> migrationProviders,
        IOptions<ConnectionStrings> options)
    {
        _migrationProviders = migrationProviders;
        _options = options;
    }

    /// <inheritdoc />
    public async Task ExecuteSingleMigrationAsync(EFCoreMigration migration)
    {
        IMigrationProvider? provider = _migrationProviders.FirstOrDefault(x => x.ProviderName.CompareProviderNames(_options.Value.ProviderName));

        if (provider is not null)
        {
            await provider.MigrateAsync(migration);
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAllMigrationsAsync()
    {
        IMigrationProvider? provider = _migrationProviders.FirstOrDefault(x => x.ProviderName.CompareProviderNames(_options.Value.ProviderName));
        if (provider is not null)
        {
            await provider.MigrateAllAsync();
        }
    }
}

/// <summary>
/// Executes Entity Framework Core migrations for Umbraco.
/// </summary>
[Obsolete("Use EFCoreMigrationExecutor instead. Scheduled for removal in Umbraco 19.")]
public class EfCoreMigrationExecutor : EFCoreMigrationExecutor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreMigrationExecutor"/> class.
    /// </summary>
    /// <param name="migrationProviders">The collection of migration providers.</param>
    /// <param name="options">The connection string options.</param>
    public EfCoreMigrationExecutor(
        IEnumerable<IMigrationProvider> migrationProviders,
        IOptions<ConnectionStrings> options)
        : base(migrationProviders, options)
    {
    }
}
