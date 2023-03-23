using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreMigrationService : IEFCoreMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public EFCoreMigrationService(UmbracoDbContextFactory umbracoDbContextFactory, ILogger<EFCoreMigrationService> logger)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
    }

    public async Task MigrateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.Database.MigrateAsync();
        });
}
