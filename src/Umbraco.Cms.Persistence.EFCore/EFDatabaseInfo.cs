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

            var isInstalled = (await db.Database.GetAppliedMigrationsAsync()).Any() || db.UmbracoUsers.Any();

            if (!isInstalled)
            {
                return DatabaseState.NotInstalled;
            }

            // Check if EFCore history table exist
            bool historyTableExists;

            if (db.Database.IsSqlite())
            {
                historyTableExists = await db.Database.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='dbo.__EFMigrationsHistory';") > 0;
            }
            else
            {
                historyTableExists = await db.Database.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = dbo.__EFMigrationsHistory AND TABLE_SCHEMA = (SELECT SCHEMA_NAME())") > 0;
            }

            if (historyTableExists is false)
            {
                return DatabaseState.NeedsUpgrade;
            }

            var needsUpgrade = (await db.Database.GetPendingMigrationsAsync()).Any();

            if (needsUpgrade)
            {
                return DatabaseState.NeedsUpgrade;
            }

            // Get package migrations already run from database, then match to pending migrations to see if we need to run any
            List<UmbracoKeyValue> keyValues = await db.UmbracoKeyValues.Where(x => x.Key.StartsWith(Constants.Conventions.Migrations.KeyValuePrefix)).ToListAsync();
            IReadOnlyDictionary<string, string?> dic = keyValues.ToDictionary(x => x.Key, x => x.Value);
            return _pendingPackageMigrations.GetPendingPackageMigrations(dic).Count > 0 ? DatabaseState.NeedsPackageMigration : DatabaseState.Ok;
        });

    public override async Task<string?> CurrentMigrationState(string key)
        => await _umbracoDbContextFactory.ExecuteWithContextAsync(async db =>
        {
            return await Task.FromResult(db.UmbracoKeyValues.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault());
        });
}
