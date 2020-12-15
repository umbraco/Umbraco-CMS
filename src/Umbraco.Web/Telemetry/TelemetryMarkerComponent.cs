using System;
using System.IO;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Install;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryMarkerComponent : IComponent
    {
        private readonly IProfilingLogger _logger;
        private readonly IRuntimeState _runtime;
        private readonly IUmbracoSettingsSection _settings;

        public TelemetryMarkerComponent(IProfilingLogger logger, IRuntimeState runtime, IUmbracoSettingsSection settings)
        {
            _logger = logger;
            _runtime = runtime;
            _settings = settings;
        }

        public void Initialize()
        {
            // Verify that XML attribute is not empty string
            // Try & get a value stored in umbracoSettings.config on the backoffice XML element ID attribute
            var backofficeIdentifierRaw = _settings.BackOffice.Id;

            if ((_runtime.Level == RuntimeLevel.Upgrade || _runtime.Level == RuntimeLevel.Install)
                && string.IsNullOrEmpty(backofficeIdentifierRaw) == false)
            {
                // No need to add Id again if already found during upgrade or install
                return;
            }

            // We are a clean install or an upgrade without the Id

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
                    _logger.Error<TelemetryMarkerComponent>("Unable to find umbracoSettings.config file to add telemetry site identifier");
                    return;
                }

                try
                {
                    var umbracoConfigXml = XDocument.Load(umbracoSettingsPath, LoadOptions.PreserveWhitespace);
                    if (umbracoConfigXml.Root != null)
                    {
                        var backofficeElement = umbracoConfigXml.Root.Element("backOffice");
                        if (backofficeElement == null)
                            return;

                        // Will add ID attribute if it does not exist
                        backofficeElement.SetAttributeValue("id", telemetrySiteIdentifier.ToString());

                        // Save file back down
                        umbracoConfigXml.Save(umbracoSettingsPath, SaveOptions.DisableFormatting);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error<TelemetryMarkerComponent>(ex, "Couldn't update UmbracoSettings.config with a backoffice with a telemetry site identifier");
                }
            }
        }

        public void Terminate()
        {
        }
    }
}
