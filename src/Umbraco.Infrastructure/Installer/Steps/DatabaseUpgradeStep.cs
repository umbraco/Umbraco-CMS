using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

/// <summary>
/// Represents an installation step responsible for performing database schema upgrades during the Umbraco installation or upgrade process.
/// </summary>
public class DatabaseUpgradeStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IRuntimeState _runtime;
    private readonly ILogger<DatabaseUpgradeStep> _logger;
    private readonly IUmbracoVersion _umbracoVersion;
    private readonly IKeyValueService _keyValueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUpgradeStep"/> class.
    /// </summary>
    /// <param name="databaseBuilder">Builds and upgrades the database schema.</param>
    /// <param name="runtime">Provides the current runtime state of the application.</param>
    /// <param name="logger">Logs information and errors related to the upgrade step.</param>
    /// <param name="umbracoVersion">Represents the current Umbraco version.</param>
    /// <param name="keyValueService">Manages persistent key-value pairs for upgrade tracking.</param>
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

    /// <summary>
    /// Asynchronously executes the database upgrade step during installation.
    /// </summary>
    /// <param name="_">The installation data (unused).</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the installation attempt.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => ExecuteInternalAsync();

    /// <summary>
    /// Asynchronously executes the database upgrade step.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing an <see cref="Attempt{InstallationResult}"/> that indicates the success or failure of the upgrade.</returns>
    public Task<Attempt<InstallationResult>> ExecuteAsync() => ExecuteInternalAsync();

    private async Task<Attempt<InstallationResult>> ExecuteInternalAsync()
    {
        _logger.LogInformation("Running 'Upgrade' service");

        var plan = new UmbracoPlan(_umbracoVersion);
        // TODO: Clear CSRF cookies with notification.

        DatabaseBuilder.Result? result = await _databaseBuilder.UpgradeSchemaAndDataAsync(plan).ConfigureAwait(false);

        if (result?.Success == false)
        {
            return FailWithMessage("The database failed to upgrade. ERROR: " + result.Message);
        }

        return Success();
    }

    /// <summary>
    /// Determines asynchronously whether the database upgrade step requires execution.
    /// </summary>
    /// <param name="model">The installation data model.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if execution is required; otherwise, false.</returns>
    public Task<bool> RequiresExecutionAsync(InstallData model) => ShouldExecute();

    /// <summary>
    /// Determines asynchronously whether the database upgrade step requires execution.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if execution is required; otherwise, false.</returns>
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
