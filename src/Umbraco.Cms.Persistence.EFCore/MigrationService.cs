using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

internal class MigrationService : IMigrationService
{
    private readonly IEfCoreScopeProvider<UmbracoEFContext> _efCoreScopeProvider;

    public MigrationService(IEfCoreScopeProvider<UmbracoEFContext> efCoreScopeProvider) => _efCoreScopeProvider = efCoreScopeProvider;

    public async Task MigrateAsync(string migrationName)
    {
        using IEfCoreScope<UmbracoEFContext> scope = _efCoreScopeProvider.CreateScope();
        await scope.MigrateDatabaseAsync(migrationName);
        scope.Complete();
    }
}
