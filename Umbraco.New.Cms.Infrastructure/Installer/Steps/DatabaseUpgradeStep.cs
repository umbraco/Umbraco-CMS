using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Installer.Steps;
using Umbraco.New.Cms.Core.Models.Installer;
using InstallSetupStep = Umbraco.New.Cms.Core.Installer.InstallSetupStep;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class DatabaseUpgradeStep : InstallSetupStep
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IRuntimeState _runtime;
    private readonly ILogger<DatabaseUpgradeStep> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IKeyValueService _keyValueService;

    public DatabaseUpgradeStep(
        DatabaseBuilder databaseBuilder,
        IRuntimeState runtime,
        ILogger<DatabaseUpgradeStep> logger,
        IUmbracoVersion umbracoVersion,
        IKeyValueService keyValueService)
        : base(
            "DatabaseUpgrade",
            50,
            InstallationType.Upgrade | InstallationType.NewInstall)
    {
        _databaseBuilder = databaseBuilder;
        _runtime = runtime;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _keyValueService = keyValueService;
    }

    public override Task ExecuteAsync(InstallData model)
    {
        _logger.LogInformation("Running 'Upgrade' service");

        var plan = new UmbracoPlan(_umbracoVersion);
        plan.AddPostMigration<ClearCsrfCookies>(); // needed when running installer (back-office)

        DatabaseBuilder.Result? result = _databaseBuilder.UpgradeSchemaAndData(plan);

        if (result?.Success == false)
        {
            throw new InstallException("The database failed to upgrade. ERROR: " + result.Message);
        }

        return Task.CompletedTask;
    }

    public override Task<bool> RequiresExecutionAsync(InstallData model)
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
