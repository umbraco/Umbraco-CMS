using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Extensions;

public static class UmbracoEFCoreServiceCollectionExtensions
{
    public delegate void DefaultEFCoreOptionsAction(DbContextOptionsBuilder options, string? providerName, string? connectionString);

    public static IServiceCollection AddUmbracoEFCoreContext<T>(this IServiceCollection services, string connectionString, string providerName, DefaultEFCoreOptionsAction? defaultEFCoreOptionsAction = null)
        where T : DbContext
    {
        defaultEFCoreOptionsAction ??= DefaultOptionsAction;

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
