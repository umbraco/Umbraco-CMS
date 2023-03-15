using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFCoreMigrationService : IEFCoreMigrationService
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private readonly ILogger<EFCoreMigrationService> _logger;

    public EFCoreMigrationService(UmbracoDbContextFactory umbracoDbContextFactory, ILogger<EFCoreMigrationService> logger)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
        _logger = logger;
    }

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
                try
                {
                    await db.Database.MigrateAsync();
                }
                catch (Exception exception)
                {
                    if (DoesContainTableExistsErrorMessage(exception) is false)
                    {
                        throw;
                    }
                }
            }
        });

    private bool DoesContainTableExistsErrorMessage(Exception exception)
    {
        switch (exception)
        {
            // This message will be expected if we already have installed umbraco before
            case SqliteException sqliteException when sqliteException.Message.Contains("""table "cmsDictionary" already exists'"""):
            case SqlException sqlServerException when sqlServerException.Message.Contains("""table "cmsDictionary" already exists'"""):
                return true;
            default:
                return false;
        }
    }
}
