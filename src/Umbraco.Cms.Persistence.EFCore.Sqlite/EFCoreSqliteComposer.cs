using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

public class EFCoreSqliteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMigrationProvider, SqliteMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, SqliteMigrationProviderSetup>();
    }
}
