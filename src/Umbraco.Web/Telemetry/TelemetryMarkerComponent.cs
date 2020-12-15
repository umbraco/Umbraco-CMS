using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Xml;
using Umbraco.Web.Install;

namespace Umbraco.Web.Telemetry
{
    public class TelemetryMarkerComponent : IComponent
    {
        private IProfilingLogger _logger;
        private IRuntimeState _runtime;
        private IUmbracoSettingsSection _settings;

        public TelemetryMarkerComponent(IProfilingLogger logger, IRuntimeState runtime, IUmbracoSettingsSection settings)
        {
            _logger = logger;
            _runtime = runtime;
            _settings = settings;
        }

        public void Initialize()
        {
            // Verify that XML attribute is not empty string
            // Try & get a value stored in umbracosettings.config on the backoffice XML element ID attribute
            var backofficeIdentifierRaw = _settings.BackOffice.Id;

            // Verify file does not exist already (if we are upgrading)
            // In a clean install we know it would not exist
            // If the site is upgraded and the file was removed it would re-create one
            // NOTE: If user removed the marker file to opt out it would re-create a new guid marker file & potentially skew
            if (_runtime.Level == RuntimeLevel.Upgrade && string.IsNullOrEmpty(backofficeIdentifierRaw) == false)
            {
                _logger.Warn<TelemetryMarkerComponent>("When upgrading, the anonymous telemetry id already existsed within UmbracoSettings.config with a value of {telemetrySiteId}", backofficeIdentifierRaw);
                return;
            }
            else if (_runtime.Level == RuntimeLevel.Install && string.IsNullOrEmpty(backofficeIdentifierRaw) == false)
            {
                // No need to log for when level is install if file exists (As this component hit several times during install process)
                return;
            }

            // We are a clean install or an upgrade without the marker file
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
                    _logger.Error<TelemetryMarkerComponent>("Unable to find UmbracoSettings.config file to add telemetry site identifier");
                    return;
                }

                try
                {
                    var umbracoConfigXml = XDocument.Load(umbracoSettingsPath, LoadOptions.PreserveWhitespace);
                    if (umbracoConfigXml.Root != null)
                    {
                        var backofficeElement = umbracoConfigXml.Root.Element("backOffice");
                        if (backofficeElement != null)
                        {
                            // Will add ID attribute if it does not exist
                            backofficeElement.SetAttributeValue("id", telemetrySiteIdentifier.ToString());

                            // Save file back down
                            umbracoConfigXml.Save(umbracoSettingsPath, SaveOptions.DisableFormatting);
                        }
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
