using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.Sqlite.Interceptors;
using Umbraco.Cms.Persistence.Sqlite.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.Sqlite;

/// <summary>
///     SQLite support extensions for IUmbracoBuilder.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Add required services for SQLite support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoSqliteSupport(this IUmbracoBuilder builder)
    {
        // TryAddEnumerable takes both TService and TImplementation into consideration (unlike TryAddSingleton)
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, SqliteSyntaxProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IBulkSqlInsertProvider, SqliteBulkSqlInsertProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, SqliteDatabaseCreator>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificMapperFactory, SqliteSpecificMapperFactory>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderMetadata, SqliteDatabaseProviderMetadata>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDistributedLockingMechanism, SqliteDistributedLockingMechanism>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificInterceptor, SqliteAddPreferDeferredInterceptor>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificInterceptor, SqliteAddMiniProfilerInterceptor>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IProviderSpecificInterceptor, SqliteAddRetryPolicyInterceptor>());

        DbProviderFactories.UnregisterFactory(Constants.ProviderName);
        DbProviderFactories.RegisterFactory(Constants.ProviderName, SqliteFactory.Instance);

        // Remove this registration in Umbraco 12
        DbProviderFactories.UnregisterFactory(Constants.ProviderNameLegacy);
        DbProviderFactories.RegisterFactory(Constants.ProviderNameLegacy, SqliteFactory.Instance);

        // Prevent accidental creation of SQLite database files
        builder.Services.PostConfigureAll<ConnectionStrings>(options =>
        {
            // Skip empty connection string and other providers
            if (!options.IsConnectionStringConfigured() || (options.ProviderName != Constants.ProviderName && options.ProviderName != Constants.ProviderNameLegacy))
            {
                return;
            }

            var connectionStringBuilder = new SqliteConnectionStringBuilder(options.ConnectionString);
            if (connectionStringBuilder.Mode == SqliteOpenMode.ReadWriteCreate)
            {
                connectionStringBuilder.Mode = SqliteOpenMode.ReadWrite;
                options.ConnectionString = connectionStringBuilder.ConnectionString;
            }
        });

        return builder;
    }
}
