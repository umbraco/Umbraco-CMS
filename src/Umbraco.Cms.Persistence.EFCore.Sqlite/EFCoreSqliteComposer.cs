using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Migrations;
using Umbraco.Extensions;


namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Composer for registering SQLite EF Core migration services.
/// </summary>
public class EFCoreSqliteComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMigrationProvider, SqliteMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, SqliteMigrationProviderSetup>();
        builder.Services.AddSingleton<IDatabaseConfigurator, SqliteDatabaseConfigurator>();

        builder.AddDbContextRegistrar<SqliteDbContextServiceRegistrar>();
    }
}
