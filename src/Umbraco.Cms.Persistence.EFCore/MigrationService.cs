using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

internal class MigrationService : IMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public MigrationService(UmbracoDbContextFactory umbracoDbContextFactory)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
    }

    public async Task MigrateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Database.MigrateAsync();
        });
}
