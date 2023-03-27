using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Installer.Steps;

public class TelemetryIdentifierStep : IInstallStep, IUpgradeStep
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

    public Task ExecuteAsync(InstallData _) => Execute();

    public Task ExecuteAsync() => Execute();

    private Task Execute()
    {
        _siteIdentifierService.TryCreateSiteIdentifier(out _);
        return Task.CompletedTask;
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
