using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class DatabaseUpgradeStep : IInstallStep, IUpgradeStep
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IRuntimeState _runtime;
    private readonly ILogger<DatabaseUpgradeStep> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IKeyValueService _keyValueService;
    private readonly IDatabaseBuilder _efCoreDatabaseBuilder;

    public DatabaseUpgradeStep(
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtime,
        ILogger<DatabaseUpgradeStep> logger,
        IUmbracoVersion umbracoVersion,
        IKeyValueService keyValueService)
        : this(
            databaseBuilder,
            runtime,
            logger,
            umbracoVersion,
            keyValueService,
            StaticServiceProvider.Instance.GetRequiredService<IDatabaseBuilder>())
    {
    }

    public DatabaseUpgradeStep(
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtime,
        ILogger<DatabaseUpgradeStep> logger,
        IUmbracoVersion umbracoVersion,
        IKeyValueService keyValueService,
        IDatabaseBuilder efCoreDatabaseBuilder)
    {
        _databaseBuilder = databaseBuilder;
        _runtime = runtime;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _keyValueService = keyValueService;
        _efCoreDatabaseBuilder = efCoreDatabaseBuilder;
    }

    public Task ExecuteAsync(InstallData _) => Execute();

    public Task ExecuteAsync() => Execute();

    private async Task Execute()
    {
        _logger.LogInformation("Running 'Upgrade' service");

        var plan = new UmbracoPlan(_umbracoVersion);
        plan.AddPostMigration<ClearCsrfCookies>(); // needed when running installer (back-office)

        DatabaseBuilder.Result? result = _databaseBuilder.UpgradeSchemaAndData(plan);

        if (result?.Success == false)
        {
            throw new InstallException("The database failed to upgrade. ERROR: " + result.Message);
        }

        await ExecuteEFCoreUpgrade();
    }

    private async Task ExecuteEFCoreUpgrade()
    {
        _logger.LogInformation("Running EFCore upgrade");
        var plan = new UmbracoEFCorePlan();
        await _efCoreDatabaseBuilder.UpgradeSchemaAndData(plan);
    }

    public Task<bool> RequiresExecutionAsync(InstallData model) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute()
    {
        // Don't do anything if RunTimeLevel is not Install/Upgrade
        if (_runtime.Level == RuntimeLevel.Run)
        {
            return Task.FromResult(false);
        }

        // Check the upgrade state, if it matches we dont have to upgrade.
        var plan = new UmbracoPlan(_umbracoVersion);
        var currentState = _keyValueService.GetValue(Constants.Conventions.Migrations.KeyValuePrefix + plan.Name);
        if (currentState != plan.FinalState)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
