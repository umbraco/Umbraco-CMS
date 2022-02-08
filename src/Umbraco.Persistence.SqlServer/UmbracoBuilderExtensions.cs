using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Persistence.SqlServer.Services;

namespace Umbraco.Persistence.SqlServer;

/// <summary>
/// SQLite support extensions for IUmbracoBuilder.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Add required services for SQL Server support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoSqlServerSupport(this IUmbracoBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, SqlServerSyntaxProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IBulkSqlInsertProvider, SqlServerBulkSqlInsertProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, SqlServerDatabaseCreator>());

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseProviderMetadata, SqlLocalDbDatabaseProviderMetadata>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseProviderMetadata, SqlServerDatabaseProviderMetadata>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseProviderMetadata, SqlAzureDatabaseProviderMetadata>());

        DbProviderFactories.UnregisterFactory(Cms.Core.Constants.DbProviderNames.SqlServer);
        DbProviderFactories.RegisterFactory(Cms.Core.Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

        return builder;
    }
}
