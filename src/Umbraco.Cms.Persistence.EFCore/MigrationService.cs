using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

internal class MigrationService : IMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public MigrationService(UmbracoDbContextFactory umbracoDbContextFactory) => _umbracoDbContextFactory = umbracoDbContextFactory;

    public async Task MigrateAsync(string migrationName) =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.GetService<IMigrator>().MigrateAsync(migrationName);
        });
}
