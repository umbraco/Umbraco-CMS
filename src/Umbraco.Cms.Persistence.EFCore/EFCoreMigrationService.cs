using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreMigrationService : IEFCoreMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public EFCoreMigrationService(UmbracoDbContextFactory umbracoDbContextFactory)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
    }

    public async Task MigrateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Database.MigrateAsync();
        });

    public async Task<string?> GetCurrentMigrationStateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync(async db => db.Database.GetMigrations().LastOrDefault());

    public async Task<string> GetFinalMigrationStateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync(
        async db =>
        {
            IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            return pendingMigrations.Last();
        });
}
