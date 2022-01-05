using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Manifest;
using Umbraco.Core.Telemetry.Models;

namespace Umbraco.Core.Telemetry
{
    /// <inheritdoc/>
    internal class TelemetryService : ITelemetryService
    {
        private readonly IUmbracoSettingsSection _settings;
        private readonly ManifestParser _manifestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        public TelemetryService(
            ManifestParser manifestParser,
            IUmbracoSettingsSection settings)
        {
            _manifestParser = manifestParser;
            _settings = settings;
        }

        /// <inheritdoc/>
        public bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData)
        {
            if (TryGetTelemetryId(out Guid telemetryId) is false)
            {
                telemetryReportData = null;
                return false;
            }

            telemetryReportData = new TelemetryReportData
            {
                Id = telemetryId,
                Version = UmbracoVersion.SemanticVersion.ToSemanticString(),
                Packages = GetPackageTelemetry()
            };
            return true;
        }

        private bool TryGetTelemetryId(out Guid telemetryId)
        {
            // Parse telemetry string as a GUID & verify its a GUID and not some random string
            // since users may have messed with or decided to empty the app setting or put in something random
            if (Guid.TryParse(_settings.BackOffice.Id, out var parsedTelemetryId) is false)
            {
                telemetryId = Guid.Empty;
                return false;
            }

            telemetryId = parsedTelemetryId;
            return true;
        }

        private IEnumerable<PackageTelemetry> GetPackageTelemetry()
        {
            List<PackageTelemetry> packages = new ();
            var manifests = _manifestParser.GetManifests();

            foreach (var manifest in manifests)
            {
                if (manifest.AllowPackageTelemetry is false)
                {
                    continue;
                }

                packages.Add(new PackageTelemetry
                {
                    Name = manifest.PackageName,
                    Version = manifest.Version ?? string.Empty
                });
            }

            return packages;
        }
    }
}
