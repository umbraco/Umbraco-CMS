using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer.Steps;

public class TelemetryIdentifierStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ISiteIdentifierService _siteIdentifierService;

    public TelemetryIdentifierStep(
        IOptions<GlobalSettings> globalSettings,
        ISiteIdentifierService siteIdentifierService)
    {
        _globalSettings = globalSettings;
        _siteIdentifierService = siteIdentifierService;
    }

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private Task<Attempt<InstallationResult>> Execute()
    {
        _siteIdentifierService.TryCreateSiteIdentifier(out _);
        return Task.FromResult(Success());
    }

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private Task<bool> ShouldExecute()
    {
        // Verify that Json value is not empty string
        // Try & get a value stored in appSettings.json
        var backofficeIdentifierRaw = _globalSettings.Value.Id;

        // No need to add Id again if already found
        return Task.FromResult(string.IsNullOrEmpty(backofficeIdentifierRaw));
    }
}
