using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Persistence.EFCore.SqlServer;

public class EFCoreSqlServerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMigrationProvider, SqlServerMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, SqlServerMigrationProviderSetup>();
    }
}
