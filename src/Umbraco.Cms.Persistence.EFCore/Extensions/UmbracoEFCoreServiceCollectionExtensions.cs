using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Persistence.EFCore.Factories;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Extensions;

public static class UmbracoEFCoreServiceCollectionExtensions
{
    public delegate void DefaultEFCoreOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString);

    public static IServiceCollection AddUmbracoEFCoreContext<T>(this IServiceCollection services, DefaultEFCoreOptionsAction? defaultEFCoreOptionsAction = null)
        where T : DbContext
    {
        defaultEFCoreOptionsAction ??= DefaultOptionsAction;

        services.AddPooledDbContextFactory<T>((serviceProvider, options) =>
        {
            SetupDbContext(defaultEFCoreOptionsAction, serviceProvider, options);
        });

        services.AddUnique<IAmbientEFCoreScopeStack<T>, AmbientEFCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEFCoreScopeProvider<T>, EFCoreScopeProvider<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<T>>();

        return services;
    }

    public static IServiceCollection AddUmbracoEFCoreContext<T>(this IServiceCollection services, string connectionString, string providerName, DefaultEFCoreOptionsAction? defaultEFCoreOptionsAction = null)
        where T : DbContext
    {
        defaultEFCoreOptionsAction ??= DefaultOptionsAction;

        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionString = connectionString.Replace(Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        services.AddPooledDbContextFactory<T>(options =>
        {
            defaultEFCoreOptionsAction(options, providerName, connectionString);
        });

        services.AddUnique<IAmbientEFCoreScopeStack<T>, AmbientEFCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEFCoreScopeProvider<T>, EFCoreScopeProvider<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<T>>();

        return services;
    }

    private static void SetupDbContext(DefaultEFCoreOptionsAction defaultEFCoreOptionsAction, IServiceProvider provider, DbContextOptionsBuilder builder)
    {
        ConnectionStrings connectionStrings = GetConnectionStringAndProviderName(provider);

        if (string.IsNullOrWhiteSpace(connectionStrings.ConnectionString))
        {
            ILogger<UmbracoDatabaseFactory> logger = provider.GetRequiredService<ILogger<UmbracoDatabaseFactory>>();
            logger.LogCritical("No connection string was found, cannot setup Umbraco EF Core context");
        }

        IEnumerable<IMigrationProviderSetup> migrationProviders = provider.GetServices<IMigrationProviderSetup>();
        IMigrationProviderSetup? migrationProvider =
            migrationProviders.FirstOrDefault(x => x.ProviderName == connectionStrings.ProviderName);

        if (migrationProvider == null && connectionStrings.ProviderName != null)
        {
            throw new InvalidOperationException($"No migration provider found for provider name {connectionStrings.ProviderName}");
        }

        migrationProvider?.Setup(builder, connectionStrings.ConnectionString);
        defaultEFCoreOptionsAction(builder, connectionStrings.ConnectionString, connectionStrings.ProviderName);
    }

    private static ConnectionStrings GetConnectionStringAndProviderName(IServiceProvider serviceProvider)
    {
        ConnectionStrings connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionStrings.ConnectionString = connectionStrings.ConnectionString?.Replace(Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        return connectionStrings;
    }

    private static void DefaultOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString)
    {
        if (connectionString.IsNullOrWhiteSpace())
        {
            return;
        }

        switch (providerName)
        {
            case "Microsoft.Data.Sqlite":
                options.UseSqlite(connectionString, optionsBuilder =>
                {
                    optionsBuilder.MigrationsAssembly("Umbraco.Cms.Persistence.EFCore.Sqlite");
                });
                break;
            case "Microsoft.Data.SqlClient":
                options.UseSqlServer(connectionString, optionsBuilder =>
                {
                    optionsBuilder.MigrationsAssembly("Umbraco.Cms.Persistence.EFCore.SqlServer");
                });
                break;

            case "":
                if (!connectionString.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException("No provider was found with your connection string please specify one");
                }
                break;
            default:
                throw new NotSupportedException($"The provider {providerName} is not supported");
        }
    }
}
