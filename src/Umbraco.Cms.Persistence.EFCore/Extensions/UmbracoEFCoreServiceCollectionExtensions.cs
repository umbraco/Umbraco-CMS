using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Persistence.EFCore.Factories;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Extensions;

public static class UmbracoEFCoreServiceCollectionExtensions
{
    public delegate void DefaultEFCoreOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString);

    public static IServiceCollection AddUmbracoEFCoreContext<T>(this IServiceCollection services, DefaultEFCoreOptionsAction? defaultEFCoreOptionsAction = null)
        where T : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        services.TryAddSingleton<IDbContextFactory<T>>(
            sp =>
            {
                SetupDbContext(defaultEFCoreOptionsAction, sp, optionsBuilder);
                return new UmbracoPooledDbContextFactory<T>(sp.GetRequiredService<IRuntimeState>(),optionsBuilder.Options);
            });
        services.AddPooledDbContextFactory<T>((provider, builder) => SetupDbContext(defaultEFCoreOptionsAction, provider, builder));
        services.AddTransient(services => services.GetRequiredService<IDbContextFactory<T>>().CreateDbContext());

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
        // Replace data directory
        string? dataDirectory = AppDomain.CurrentDomain.GetData(Constants.System.DataDirectoryName)?.ToString();
        if (string.IsNullOrEmpty(dataDirectory) is false)
        {
            connectionString = connectionString.Replace(Constants.System.DataDirectoryPlaceholder, dataDirectory);
        }

        var optionsBuilder = new DbContextOptionsBuilder<T>();
        services.TryAddSingleton<IDbContextFactory<T>>(
            sp =>
            {
                SetupDbContext(defaultEFCoreOptionsAction, sp, optionsBuilder);
                return new UmbracoPooledDbContextFactory<T>(sp.GetRequiredService<IRuntimeState>(),optionsBuilder.Options);
            });
        services.AddPooledDbContextFactory<T>(options => defaultEFCoreOptionsAction?.Invoke(options, providerName, connectionString));
        services.AddTransient(services => services.GetRequiredService<IDbContextFactory<T>>().CreateDbContext());

        services.AddUnique<IAmbientEFCoreScopeStack<T>, AmbientEFCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEFCoreScopeProvider<T>, EFCoreScopeProvider<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<T>>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<T>>();

        return services;
    }

    private static void SetupDbContext(DefaultEFCoreOptionsAction? defaultEFCoreOptionsAction, IServiceProvider provider, DbContextOptionsBuilder builder)
    {
        ConnectionStrings connectionStrings = GetConnectionStringAndProviderName(provider);
        defaultEFCoreOptionsAction?.Invoke(builder, connectionStrings.ConnectionString, connectionStrings.ProviderName);
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
}
