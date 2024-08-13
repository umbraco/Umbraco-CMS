using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public class DatabaseUpgradeStep : StepBase, IInstallStep, IUpgradeStep
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
    {
        _databaseBuilder = databaseBuilder;
        _runtime = runtime;
        _logger = logger;
        _umbracoVersion = umbracoVersion;
        _keyValueService = keyValueService;
    }

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => ExecuteInternalAsync();

    public Task<Attempt<InstallationResult>> ExecuteAsync() => ExecuteInternalAsync();

    private Task<Attempt<InstallationResult>> ExecuteInternalAsync()
    {
        _logger.LogInformation("Running 'Upgrade' service");

        var plan = new UmbracoPlan(_umbracoVersion);
        // TODO: Clear CSRF cookies with notification.

        DatabaseBuilder.Result? result = _databaseBuilder.UpgradeSchemaAndData(plan);

        if (result?.Success == false)
        {
            return Task.FromResult(FailWithMessage("The database failed to upgrade. ERROR: " + result.Message));
        }

        return Task.FromResult(Success());
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
