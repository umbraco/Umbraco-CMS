using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFDatabaseInfo : DatabaseInfoBase
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private readonly PendingPackageMigrations _pendingPackageMigrations;

    public EFDatabaseInfo(
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        UmbracoDbContextFactory umbracoDbContextFactory,
        PendingPackageMigrations pendingPackageMigrations)
        : base(connectionStrings)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
        _pendingPackageMigrations = pendingPackageMigrations;
    }

    protected override async Task<DatabaseState> GetConfiguredStateAsync() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync(async db =>
        {
            if (!await db.Database.CanConnectAsync())
            {
                return DatabaseState.CannotConnect;
            }

            var anyPendingMigrations = (await db.Database.GetPendingMigrationsAsync()).Any();

            if (anyPendingMigrations)
            {
                // Check if umbracoKeyValue table exists
                bool keyValueTableExists;
                if (db.Database.IsSqlite())
                {
                    keyValueTableExists = await db.Database.ExecuteScalarAsync<int>(
                        $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name LIKE '%umbracoKeyValue%';") > 0;
                }
                else
                {
                    keyValueTableExists = await db.Database.ExecuteScalarAsync<int>(
                                              $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%umbracoKeyValue%' AND TABLE_SCHEMA = (SELECT SCHEMA_NAME())") >
                                          0;
                }

                return keyValueTableExists
                    ? DatabaseState.NeedsUpgrade
                    :
                    // If there are pending migrations but no umbracoKeyValue table, it must mean its an empty database.
                    DatabaseState.NotInstalled;
            }

            // Get package migrations already run from database, then match to pending migrations to see if we need to run any
            List<UmbracoKeyValue> keyValues = await db.UmbracoKeyValues
                .Where(x => x.Key.StartsWith(Constants.Conventions.Migrations.KeyValuePrefix)).ToListAsync();
            IReadOnlyDictionary<string, string?> dic = keyValues.ToDictionary(x => x.Key, x => x.Value);
            return _pendingPackageMigrations.GetPendingPackageMigrations(dic).Count > 0
                ? DatabaseState.NeedsPackageMigration
                : DatabaseState.Ok;
        });

    public override async Task<string?> CurrentMigrationState(string key) =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync(async db =>
        {
            var migrationState = db.UmbracoKeyValues.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault();

            if (migrationState is null)
            {
                IEnumerable<string> appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
                migrationState = appliedMigrations.LastOrDefault();
            }

            return migrationState;
        });

    public override async Task<string> FinalMigrationState() =>
        await _umbracoDbContextFactory.ExecuteWithContextAsync(
            async db =>
            {
                IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                return pendingMigrations.Last();
            });
}
