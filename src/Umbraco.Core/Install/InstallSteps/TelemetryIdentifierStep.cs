using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "TelemetryIdConfiguration", 0, "",
        PerformsAppRestart = false)]
    public class TelemetryIdentifierStep : InstallSetupStep<object>
    {
        private readonly ILogger<TelemetryIdentifierStep> _logger;
        private readonly IOptions<GlobalSettings> _globalSettings;
        private readonly IConfigManipulator _configManipulator;

        public TelemetryIdentifierStep(ILogger<TelemetryIdentifierStep> logger, IOptions<GlobalSettings> globalSettings, IConfigManipulator configManipulator)
        {
            _logger = logger;
            _globalSettings = globalSettings;
            _configManipulator = configManipulator;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            // Generate GUID
            var telemetrySiteIdentifier = Guid.NewGuid();

            try
            {
                _configManipulator.SetGlobalId(telemetrySiteIdentifier.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't update config files with a telemetry site identifier");
            }

            return Task.FromResult<InstallSetupResult>(null);
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
}
