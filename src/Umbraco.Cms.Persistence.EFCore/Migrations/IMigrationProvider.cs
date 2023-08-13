using Umbraco.Cms.Core.Attributes;

namespace Umbraco.Cms.Persistence.EFCore.Migrations;

[UmbracoInternal]
public interface IMigrationProvider
{
    string ProviderName { get; }

    Task MigrateAsync(EFCoreMigration migration);

    Task MigrateAllAsync();
}
