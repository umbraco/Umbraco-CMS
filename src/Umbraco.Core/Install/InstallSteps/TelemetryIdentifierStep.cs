using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.Install.InstallSteps;

[Obsolete("Will be replace with a new step with the new backoffice")]
[InstallSetupStep(
    InstallationType.NewInstall | InstallationType.Upgrade,
    "TelemetryIdConfiguration",
    0,
    "",
    PerformsAppRestart = false)]
public class TelemetryIdentifierStep : InstallSetupStep<object>
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

    [Obsolete("Use constructor that takes GlobalSettings and ISiteIdentifierService")]
    public TelemetryIdentifierStep(
        ILogger<TelemetryIdentifierStep> logger,
        IOptions<GlobalSettings> globalSettings,
        IConfigManipulator configManipulator)
        : this(globalSettings, StaticServiceProvider.Instance.GetRequiredService<ISiteIdentifierService>())
    {
    }

    public override Task<InstallSetupResult?> ExecuteAsync(object model)
    {
        _siteIdentifierService.TryCreateSiteIdentifier(out _);
        return Task.FromResult<InstallSetupResult?>(null);
    }

    public override bool RequiresExecution(object model)
    {
        // Verify that Json value is not empty string
        // Try & get a value stored in appSettings.json
        var backofficeIdentifierRaw = _globalSettings.Value.Id;

        // No need to add Id again if already found
        return string.IsNullOrEmpty(backofficeIdentifierRaw);
    }
}
