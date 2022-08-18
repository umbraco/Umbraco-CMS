using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.SqlServer.Interceptors;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Persistence.SqlServer;

/// <summary>
///     SQLite support extensions for IUmbracoBuilder.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Add required services for SQL Server support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoSqlServerSupport(this IUmbracoBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, SqlServerSyntaxProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IBulkSqlInsertProvider, SqlServerBulkSqlInsertProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, SqlServerDatabaseCreator>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderMetadata, SqlLocalDbDatabaseProviderMetadata>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderMetadata, SqlServerDatabaseProviderMetadata>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderMetadata, SqlAzureDatabaseProviderMetadata>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDistributedLockingMechanism, SqlServerDistributedLockingMechanism>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificInterceptor, SqlServerAddMiniProfilerInterceptor>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificInterceptor, SqlServerAddRetryPolicyInterceptor>());

        DbProviderFactories.UnregisterFactory(Constants.ProviderName);
        DbProviderFactories.RegisterFactory(Constants.ProviderName, SqlClientFactory.Instance);

        // Support provider name set by the configuration API for connection string environment variables
        builder.Services.ConfigureAll<ConnectionStrings>(options =>
        {
            if (options.ProviderName == "System.Data.SqlClient")
            {
                options.ProviderName = Constants.ProviderName;
            }
        });

        return builder;
    }
}
