using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.Migrations;
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
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMigrationProvider, SqlServerMigrationProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMigrationProviderSetup, SqlServerMigrationProviderSetup>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseConfigurator, SqlServerDatabaseConfigurator>());

        builder.AddDbContextRegistrar<SqlServerDbContextServiceRegistrar>();

        return builder;
    }
}
