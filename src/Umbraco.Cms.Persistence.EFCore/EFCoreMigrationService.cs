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
                /* If we have no history table, it is safe to assume EFCore is not installed in the database
                   When trying to migrate we can be in 1 of 2 states:
                   State 1: We have an empty database
                   State 2: We have a database with all the umbraco tables
                   If we are in State 2, the migration will try to add these tables,
                   but it can't as the tables already exists. Therefore we have to wrap this in a try/catch to catch the
                   "Table already exists" exception. Luckily the MigrateAsync() method will add the __EFCoreHistoryTable
                   So this works just fine, but is just a bit hacky.
                */
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
