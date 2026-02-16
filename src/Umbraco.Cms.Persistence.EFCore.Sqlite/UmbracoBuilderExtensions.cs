using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// SQLite EF Core support extensions for <see cref="IUmbracoBuilder"/>.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Add required services for SQLite EF Core support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoEFCoreSqliteSupport(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMigrationProvider, SqliteMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, SqliteMigrationProviderSetup>();
        builder.Services.AddSingleton<IDatabaseConfigurator, SqliteDatabaseConfigurator>();

        builder.AddDbContextRegistrar<SqliteDbContextServiceRegistrar>();

        return builder;
    }
}
