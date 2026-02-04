using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

/// <summary>
/// An installation and upgrade step that creates a unique telemetry site identifier.
/// </summary>
public class TelemetryIdentifierStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ISiteIdentifierService _siteIdentifierService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryIdentifierStep"/> class.
    /// </summary>
    /// <param name="globalSettings">The global settings containing the current site identifier.</param>
    /// <param name="siteIdentifierService">The service used to create site identifiers.</param>
    public TelemetryIdentifierStep(
        IOptions<GlobalSettings> globalSettings,
        ISiteIdentifierService siteIdentifierService)
    {
        _globalSettings = globalSettings;
        _siteIdentifierService = siteIdentifierService;
    }

    /// <inheritdoc />
    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    /// <inheritdoc />
    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    /// <summary>
    /// Executes the telemetry identifier creation.
    /// </summary>
    /// <returns>A task containing an attempt with the installation result.</returns>
    private Task<Attempt<InstallationResult>> Execute()
    {
        _siteIdentifierService.TryCreateSiteIdentifier(out _);
        return Task.FromResult(Success());
    }

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    /// <inheritdoc />
    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    /// <summary>
    /// Determines whether this step should be executed based on whether a site identifier already exists.
    /// </summary>
    /// <returns>A task containing <c>true</c> if no site identifier exists; otherwise, <c>false</c>.</returns>
    private Task<bool> ShouldExecute()
    {
        // Verify that Json value is not empty string
        // Try & get a value stored in appSettings.json
        var backofficeIdentifierRaw = _globalSettings.Value.Id;

        // No need to add Id again if already found
        return Task.FromResult(string.IsNullOrEmpty(backofficeIdentifierRaw));
    }
}
