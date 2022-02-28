using System;
using System.Collections.Generic;
using Umbraco.Core.Configuration;
using Umbraco.Core.Manifest;
using Umbraco.Core.Telemetry.Models;

namespace Umbraco.Core.Telemetry
{
    /// <inheritdoc/>
    internal class TelemetryService : ITelemetryService
    {
        private readonly ISiteIdentifierService _siteIdentifierService;
        private readonly ManifestParser _manifestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        public TelemetryService(
            ManifestParser manifestParser,
            ISiteIdentifierService siteIdentifierService)
        {
            _manifestParser = manifestParser;
            _siteIdentifierService = siteIdentifierService;
        }

        /// <inheritdoc/>
        public bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData)
        {
            if (_siteIdentifierService.TryGetOrCreateSiteIdentifier(out Guid telemetryId) is false)
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
