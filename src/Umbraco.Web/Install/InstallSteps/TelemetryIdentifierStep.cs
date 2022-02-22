using System.Threading.Tasks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Telemetry;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "TelemetryIdConfiguration", 0, "",
        PerformsAppRestart = false)]
    internal class TelemetryIdentifierStep : InstallSetupStep<object>
    {
        private readonly IUmbracoSettingsSection _settings;
        private readonly ISiteIdentifierService _siteIdentifierService;

        public TelemetryIdentifierStep(
            IUmbracoSettingsSection settings,
            ISiteIdentifierService siteIdentifierService)
        {
            _settings = settings;
            _siteIdentifierService = siteIdentifierService;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            _siteIdentifierService.TryCreateSiteIdentifier(out _);
            return Task.FromResult<InstallSetupResult>(null);
        }

        public override bool RequiresExecution(object model)
        {
            // Verify that XML attribute is not empty string
            // Try & get a value stored in umbracoSettings.config on the backoffice XML element ID attribute
            var backofficeIdentifierRaw = _settings.BackOffice.Id;

            // No need to add Id again if already found
            return string.IsNullOrEmpty(backofficeIdentifierRaw);
        }
    }
}
