using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreMigrationService : IEFCoreMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public EFCoreMigrationService(UmbracoDbContextFactory umbracoDbContextFactory)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
    }

    public async Task AddHistoryTable() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Database.MigrateAsync();
        });
}
