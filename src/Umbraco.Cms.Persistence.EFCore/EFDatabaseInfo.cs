using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence;

namespace Umbraco.Cms.Persistence.EFCore;

public class EFDatabaseInfo : DatabaseInfoBase
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private readonly ILogger<EFDatabaseInfo> _logger;

    public EFDatabaseInfo(
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        UmbracoDbContextFactory umbracoDbContextFactory,
        ILogger<EFDatabaseInfo> logger)
        : base(connectionStrings)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
        _logger = logger;
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

                    //TODO DatabaseState.NeedsPackageMigration

                    return DatabaseState.Ok;
                }

                return DatabaseState.CannotConnect;
            });
    }
}
