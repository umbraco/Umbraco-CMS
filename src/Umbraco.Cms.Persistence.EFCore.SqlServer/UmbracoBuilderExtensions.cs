using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.SqlServer.DtoCustomization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

/// <summary>
/// SQL Server EF Core support extensions for <see cref="IUmbracoBuilder"/>.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Add required services for SQL Server EF Core support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoEFCoreSqlServerSupport(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMigrationProvider, SqlServerMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, SqlServerMigrationProviderSetup>();
        builder.Services.AddSingleton<IDatabaseConfigurator, SqlServerDatabaseConfigurator>();

        builder.AddDbContextRegistrar<SqlServerDbContextServiceRegistrar>();

        builder.AddEFCoreModelCustomizer<SqlServerNodeDtoModelCustomizer>();

        return builder;
    }
}
