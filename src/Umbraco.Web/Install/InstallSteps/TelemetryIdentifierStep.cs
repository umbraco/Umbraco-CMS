using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "TelemetryIdConfiguration", 0, "",
        PerformsAppRestart = false)]
    internal class TelemetryIdentifierStep : InstallSetupStep<object>
    {
        private readonly IProfilingLogger _logger;
        private readonly IUmbracoSettingsSection _settings;

        public TelemetryIdentifierStep(IProfilingLogger logger, IUmbracoSettingsSection settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            // Generate GUID
            var telemetrySiteIdentifier = Guid.NewGuid();

            // Modify the XML to add a new GUID site identifier
            // hack: ensure this does not trigger a restart
            using (ChangesMonitor.Suspended())
            {
                var umbracoSettingsPath = IOHelper.MapPath(SystemFiles.UmbracoSettings);
                if(File.Exists(umbracoSettingsPath) == false)
                {
                    // Log an error
                    _logger.Error<TelemetryIdentifierStep>("Unable to find umbracoSettings.config file to add telemetry site identifier");
                    return Task.FromResult<InstallSetupResult>(null);
                }

                try
                {
                    var umbracoConfigXml = XDocument.Load(umbracoSettingsPath, LoadOptions.PreserveWhitespace);
                    if (umbracoConfigXml.Root != null)
                    {
                        var backofficeElement = umbracoConfigXml.Root.Element("backOffice");
                        if (backofficeElement == null)
                            return Task.FromResult<InstallSetupResult>(null);

                        // Will add ID attribute if it does not exist
                        backofficeElement.SetAttributeValue("id", telemetrySiteIdentifier.ToString());

                        // Save file back down
                        umbracoConfigXml.Save(umbracoSettingsPath, SaveOptions.DisableFormatting);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error<TelemetryIdentifierStep>(ex, "Couldn't update umbracoSettings.config with a backoffice with a telemetry site identifier");
                }
            }

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
