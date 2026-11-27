using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.Scoping;

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
        builder.Services.AddSingleton<IEFCoreSharedConnectionInitializer, SqliteSharedConnectionInitializer>();
    }
}
