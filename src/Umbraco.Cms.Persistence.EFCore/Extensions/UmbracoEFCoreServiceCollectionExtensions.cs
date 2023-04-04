using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Extensions;

public static class UmbracoEFCoreServiceCollectionExtensions
{
    public static IServiceCollection AddUmbracoEFCore(this IServiceCollection services, IConfiguration configuration, Action<DbContextOptionsBuilder, IConfiguration>? optionsAction = null)
    {
        services.AddDbContext<UmbracoEFContext>(
            options =>
        {
            if (optionsAction is not null)
            {
                optionsAction(options, configuration);
            }
            else
            {
                DefaultOptionsAction(options, configuration);
            }
        },
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<UmbracoEFContext>(options =>
        {
            if (optionsAction is not null)
            {
                optionsAction(options, configuration);
            }
            else
            {
                DefaultOptionsAction(options, configuration);
            }
        });

        services.AddUnique<IDatabaseInfo, EFDatabaseInfo>();
        services.AddUnique<IDatabaseSchemaCreatorFactory, EFDatabaseSchemaCreatorFactory>();
        services.AddUnique<IDatabaseDataCreator, EFCoreDatabaseDataCreator>();
        services.AddSingleton<UmbracoDbContextFactory>();
        services.AddUnique<IUmbracoEfCoreDatabaseFactory, UmbracoEfCoreDatabaseFactory>();
        services.AddUnique<IAmbientEfCoreScopeStack<UmbracoEFContext>, AmbientEfCoreScopeStack<UmbracoEFContext>>();
        services.AddUnique<IEFCoreScopeAccessor<UmbracoEFContext>, EFCoreScopeAccessor<UmbracoEFContext>>();
        services.AddUnique<IEfCoreScopeProvider<UmbracoEFContext>, EfCoreScopeProvider<UmbracoEFContext>>();
        services.AddUnique<IAmbientEFCoreScopeContextStack, AmbientEFCoreScopeContextStack>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism>();
        services.AddSingleton<IMigrationService, MigrationService>();
        return services;
    }

    public static IServiceCollection AddUmbracoEfCoreContext<T>(this IServiceCollection services, IConfiguration configuration) where T : DbContext
    {
        services.AddDbContext<T>(
            options =>
            {
                string? connectionString = configuration.GetUmbracoConnectionString("umbracoDbDSN", out string? providerName);
                if (!connectionString.IsNullOrWhiteSpace())
                {
                    if (providerName == "Microsoft.Data.Sqlite")
                    {
                        options.UseSqlite(connectionString);
                    }
                    else if (providerName == "Microsoft.Data.SqlClient")
                    {
                        options.UseSqlServer(connectionString);
                    }
                }
            },
            optionsLifetime: ServiceLifetime.Singleton);

        services.AddDbContextFactory<T>(options =>
        {
            string? connectionString = configuration.GetUmbracoConnectionString("umbracoDbDSN", out string? providerName);
            if (!connectionString.IsNullOrWhiteSpace())
            {
                if (providerName == "Microsoft.Data.Sqlite")
                {
                    options.UseSqlite(connectionString);
                }
                else if (providerName == "Microsoft.Data.SqlClient")
                {
                    options.UseSqlServer(connectionString);
                }
            }
        });

        services.AddUnique<IAmbientEfCoreScopeStack<T>, AmbientEfCoreScopeStack<T>>();
        services.AddUnique<IEFCoreScopeAccessor<T>, EFCoreScopeAccessor<T>>();
        services.AddUnique<IEfCoreScopeProvider<T>, EfCoreScopeProvider<T>>();

        return services;
    }

    private static void DefaultOptionsAction(DbContextOptionsBuilder options, IConfiguration configuration)
    {
        string? connectionString = configuration.GetUmbracoConnectionString("umbracoDbDSN", out string? providerName);
        if (!connectionString.IsNullOrWhiteSpace())
        {
            if (providerName == "Microsoft.Data.Sqlite")
            {
                options.UseSqlite(connectionString!);
            }
            else if (providerName == "Microsoft.Data.SqlClient")
            {
                options.UseSqlServer(connectionString!);
            }
        }
    }
}
