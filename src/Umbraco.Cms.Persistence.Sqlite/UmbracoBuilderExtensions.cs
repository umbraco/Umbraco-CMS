using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Persistence.Sqlite.Services;

namespace Umbraco.Cms.Persistence.Sqlite;

/// <summary>
/// SQLite support extensions for IUmbracoBuilder.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Add required services for SQLite support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoSqliteSupport(this IUmbracoBuilder builder)
    {
        // TryAddEnumerable takes both TService and TImplementation into consideration (unlike TryAddSingleton)
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, SqliteSyntaxProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IBulkSqlInsertProvider, SqliteBulkSqlInsertProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, SqliteDatabaseCreator>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProviderSpecificMapperFactory, SqliteSpecificMapperFactory>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseProviderMetadata, SqliteDatabaseProviderMetadata>());

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDistributedLockingMechanism, SqliteDistributedLockingMechanism>());

        DbProviderFactories.UnregisterFactory(Constants.ProviderName);
        DbProviderFactories.RegisterFactory(Constants.ProviderName, Microsoft.Data.Sqlite.SqliteFactory.Instance);

        return builder;
    }
}
