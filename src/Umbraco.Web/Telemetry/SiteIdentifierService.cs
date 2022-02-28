using System;
using System.IO;
using System.Xml.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Telemetry;
using Umbraco.Web.Install;

namespace Umbraco.Web.Telemetry
{
    internal class SiteIdentifierService : ISiteIdentifierService
    {
        private readonly IUmbracoSettingsSection _settings;
        private readonly ILogger _logger;

        public SiteIdentifierService(IUmbracoSettingsSection settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public bool TryGetSiteIdentifier(out Guid siteIdentifier)
        {
            // Parse telemetry string as a GUID & verify its a GUID and not some random string
            // since users may have messed with or decided to empty the app setting or put in something random
            if (Guid.TryParse(_settings.BackOffice.Id, out var parsedTelemetryId) is false
                || parsedTelemetryId == Guid.Empty)
            {
                siteIdentifier = Guid.Empty;
                return false;
            }

            siteIdentifier = parsedTelemetryId;
            return true;
        }

        public bool TryGetOrCreateSiteIdentifier(out Guid siteIdentifier)
        {
            if (TryGetSiteIdentifier(out var existingId))
            {
                siteIdentifier = existingId;
                return true;
            }

            if (TryCreateSiteIdentifier(out var createdId))
            {
                siteIdentifier = createdId;
                return true;
            }

            siteIdentifier = Guid.Empty;
            return false;
        }

        public bool TryCreateSiteIdentifier(out Guid createdGuid)
        {
            createdGuid = Guid.NewGuid();

            // Modify the XML to add a new GUID site identifier
            // hack: ensure this does not trigger a restart
            using (ChangesMonitor.Suspended())
            {
                var umbracoSettingsPath = IOHelper.MapPath(SystemFiles.UmbracoSettings);

                if (File.Exists(umbracoSettingsPath) is false)
                {
                    _logger.Error<SiteIdentifierService>("Unable to find umbracoSettings.config file to add telemetry site identifier");
                    return false;
                }

                try
                {
                    var umbracoConfigXml = XDocument.Load(umbracoSettingsPath, LoadOptions.PreserveWhitespace);
                    if (umbracoConfigXml.Root != null)
                    {
                        var backofficeElement = umbracoConfigXml.Root.Element("backOffice");
                        if (backofficeElement is null)
                        {
                            return false;
                        }

                        // Will add ID attribute if it does not exist
                        backofficeElement.SetAttributeValue("id", createdGuid.ToString());

                        // Save file back down
                        umbracoConfigXml.Save(umbracoSettingsPath, SaveOptions.DisableFormatting);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error<SiteIdentifierService>(ex, "Couldn't update umbracoSettings.config with a backoffice with a telemetry site identifier");
                    return false;
                }
            }

            return true;
        }
    }
}
