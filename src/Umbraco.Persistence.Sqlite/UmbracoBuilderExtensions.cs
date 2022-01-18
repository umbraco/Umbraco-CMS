using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Persistence.Sqlite.Services;

namespace Umbraco.Persistence.Sqlite;

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
        if (DbProviderFactories.TryGetFactory(Constants.ProviderName, out _))
        {
            return builder;
        }

        builder.Services.AddSingleton<ISqlSyntaxProvider, SqliteSyntaxProvider>();
        builder.Services.AddSingleton<IBulkSqlInsertProvider, SqliteBulkSqlInsertProvider>();
        builder.Services.AddSingleton<IDatabaseCreator, SqliteDatabaseCreator>();
        builder.Services.AddSingleton<IProviderSpecificMapperFactory, SqliteSpecificMapperFactory>();

        DbProviderFactories.RegisterFactory(Constants.ProviderName, Microsoft.Data.Sqlite.SqliteFactory.Instance);

        return builder;
    }
}
