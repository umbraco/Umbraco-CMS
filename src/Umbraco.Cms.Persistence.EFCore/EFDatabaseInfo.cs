using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;

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

    protected override async Task<DatabaseState> GetConfiguredStateAsync()
    {
        return await _umbracoDbContextFactory.ExecuteWithContextAsync(async db =>
            {
                if (await db.Database.CanConnectAsync())
                {
                    var isInstalled = (await db.Database.GetAppliedMigrationsAsync()).Any();

                    if (!isInstalled)
                    {
                        return DatabaseState.NotInstalled;
                    }

                    var needsUpgrade = (await db.Database.GetPendingMigrationsAsync()).Any();

                    if (needsUpgrade)
                    {
                        return DatabaseState.NeedsUpgrade;
                    }

                    // Get package migrations already run from database, then match to pending migrations to see if we need to run any
                    List<UmbracoKeyValue> keyValues = await db.UmbracoKeyValues.Where(x => x.Key.StartsWith(Constants.Conventions.Migrations.KeyValuePrefix)).ToListAsync();
                    IReadOnlyDictionary<string, string?> dic = keyValues.ToDictionary(x => x.Key, x => x.Value);
                    if (_pendingPackageMigrations.GetPendingPackageMigrations(dic).Count > 0)
                    {
                        return DatabaseState.NeedsPackageMigration;
                    }

                    return DatabaseState.Ok;
                }

                return DatabaseState.CannotConnect;
            });
    }
}
