using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
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

        services.AddDbContext<T>(
            (provider, builder) =>
            {
                ConnectionStrings connectionStrings = GetConnectionStringAndProviderName(provider);
                IEnumerable<IMigrationProviderSetup> migrationProviders = provider.GetServices<IMigrationProviderSetup>();
                IMigrationProviderSetup? migrationProvider = migrationProviders.FirstOrDefault(x => x.ProviderName == connectionStrings.ProviderName);

                migrationProvider?.Setup(builder, connectionStrings.ConnectionString);
                defaultEFCoreOptionsAction(builder, connectionStrings.ConnectionString, connectionStrings.ProviderName);
            },
            optionsLifetime: ServiceLifetime.Transient);



        services.AddDbContextFactory<T>((provider, builder) =>
        {
            var connectionStrings = GetConnectionStringAndProviderName(provider);
            var migrationProviders = provider.GetServices<IMigrationProviderSetup>();
            var migrationProvider = migrationProviders.FirstOrDefault(x => x.ProviderName == connectionStrings.ProviderName);
            migrationProvider?.Setup(builder, connectionStrings.ConnectionString);
            defaultEFCoreOptionsAction(builder, connectionStrings.ConnectionString, connectionStrings.ProviderName);

        });

        services.AddUnique<IAmbientEFCoreScopeStack<T>, AmbientEFCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEFCoreScopeProvider<T>, EFCoreScopeProvider<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<T>>();

        return services;
    }

    private static ConnectionStrings GetConnectionStringAndProviderName(IServiceProvider serviceProvider)
    {
        string? connectionString = null;
        string? providerName = null;

        var connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionStrings.ConnectionString = connectionStrings.ConnectionString?.Replace(Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        return connectionStrings;
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

        services.AddDbContext<T>(
            options =>
            {
                defaultEFCoreOptionsAction(options, providerName, connectionString);
            },
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<T>(options =>
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

    private static void DefaultOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString)
    {
        if (connectionString.IsNullOrWhiteSpace())
        {
            return;
        }

        switch (providerName)
        {
            case "Microsoft.Data.Sqlite":
                options.UseSqlite(connectionString);
                break;
            case "Microsoft.Data.SqlClient":
                options.UseSqlServer(connectionString);
                break;
        }
    }
}
