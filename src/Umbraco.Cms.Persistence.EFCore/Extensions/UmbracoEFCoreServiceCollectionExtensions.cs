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
        services.AddUnique<IAmbientEfCoreScopeStack, AmbientEfCoreScopeStack>();
        services.AddUnique<IEFCoreScopeAccessor, EFCoreScopeAccessor>();
        services.AddUnique<IEfCoreScopeProvider, EfCoreScopeProvider>();
        services.AddUnique<IHttpEFCoreScopeReference, HttpEFCoreScopeReference>();
        services.AddUnique<IAmbientEFCoreScopeContextStack, AmbientEFCoreScopeContextStack>();
        services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism>();
        services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism>();
        services.AddSingleton<IEFCoreMigrationService, EFCoreMigrationService>();
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
