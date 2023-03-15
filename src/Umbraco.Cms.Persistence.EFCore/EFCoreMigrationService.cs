using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreMigrationService : IEFCoreMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;

    public EFCoreMigrationService(UmbracoDbContextFactory umbracoDbContextFactory) => _umbracoDbContextFactory = umbracoDbContextFactory;

    public async Task AddHistoryTable() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync<Task>(async db =>
        {
            bool historyTableExists;
            if (db.Database.IsSqlite())
            {
                historyTableExists = await db.Database.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';") > 0;
            }
            else
            {
                historyTableExists = await db.Database.ExecuteScalarAsync<long>(
                        $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = __EFMigrationsHistory AND TABLE_SCHEMA = (SELECT SCHEMA_NAME())") > 0;
            }

            if (historyTableExists is false)
            {
                await db.Database.MigrateAsync();
            }
        });
}
