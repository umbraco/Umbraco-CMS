using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Persistence.EFCore.Install;

/// <summary>
/// EF Core implementation of database builder
/// This class contains methods, that are useful when installing & upgrading Umbraco.
/// </summary>
public class EfCoreDatabaseBuilder : IDatabaseBuilder
{
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private readonly ILogger<EfCoreDatabaseBuilder> _logger;
    private readonly IEFCoreMigrationPlanExecutor _efCoreMigrationPlanExecutor;
    private readonly IKeyValueService _keyValueService;

    /// <summary>
    /// Constructs an instance of <see cref="EfCoreDatabaseBuilder"/>
    /// </summary>
    /// <param name="umbracoDbContextFactory"></param>
    /// <param name="logger"></param>
    /// <param name="efCoreMigrationPlanExecutor"></param>
    /// <param name="keyValueService"></param>
    public EfCoreDatabaseBuilder(
        UmbracoDbContextFactory umbracoDbContextFactory,
        ILogger<EfCoreDatabaseBuilder> logger,
        IEFCoreMigrationPlanExecutor efCoreMigrationPlanExecutor,
        IKeyValueService keyValueService)
    {
        _umbracoDbContextFactory = umbracoDbContextFactory;
        _logger = logger;
        _efCoreMigrationPlanExecutor = efCoreMigrationPlanExecutor;
        _keyValueService = keyValueService;
    }

    /// <summary>
    /// Upgrades the database schema and the data in the database.
    /// </summary>
    /// <param name="plan">The migration plan to execute</param>
    public async Task UpgradeSchemaAndData(UmbracoEFCorePlan plan)
    {
        if (await ReadyForInstall() is false)
        {
            return;
        }

        _logger.LogInformation("Database upgrade started");
        var upgrader = new EFCoreUpgrader(plan);
        upgrader.Execute(_efCoreMigrationPlanExecutor, _keyValueService);

        _logger.LogInformation("Database configuration status: Upgrade completed!");
    }

    private async Task<bool> ReadyForInstall() => await _umbracoDbContextFactory.ExecuteWithContextAsync(async umbracoDbContext => await umbracoDbContext.Database.CanConnectAsync());
}
