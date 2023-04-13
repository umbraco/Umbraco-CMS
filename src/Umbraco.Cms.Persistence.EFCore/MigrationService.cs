using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

internal class MigrationService : IMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public MigrationService(UmbracoDbContextFactory umbracoDbContextFactory) => _umbracoDbContextFactory = umbracoDbContextFactory;

    public async Task MigrateAsync(string migrationName) =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.MigrateDatabaseAsync(migrationName);
        });
}
