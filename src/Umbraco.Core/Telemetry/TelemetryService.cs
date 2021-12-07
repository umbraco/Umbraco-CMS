using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <summary>
    /// Service which gathers the data for telemetry reporting
    /// </summary>
    public sealed class TelemetryService
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IManifestParser _manifestParser;
        private readonly IUmbracoVersion _umbracoVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        public TelemetryService(
            IOptions<GlobalSettings> globalSettings,
            IManifestParser manifestParser,
            IUmbracoVersion umbracoVersion)
        {
            _globalSettings = globalSettings.Value;
            _manifestParser = manifestParser;
            _umbracoVersion = umbracoVersion;
        }

        /// <summary>
        /// Try and get the <see cref="TelemetryReportData"/>
        /// </summary>
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
                Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                Packages = GetPackageTelemetry()
            };
            return true;
        }

        private bool TryGetTelemetryId(out Guid telemetryId)
        {
            // Parse telemetry string as a GUID & verify its a GUID and not some random string
            // since users may have messed with or decided to empty the app setting or put in something random
            if (Guid.TryParse(_globalSettings.Id, out var parsedTelemetryId) is false)
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
            IEnumerable<PackageManifest> manifests = _manifestParser.GetManifests();

            foreach (PackageManifest manifest in manifests)
            {
                if (manifest.AllowPackageTelemetry is false)
                {
                    continue;
                }

                packages.Add(new PackageTelemetry
                {
                    Name = manifest.PackageName,
                    Version = _globalSettings.RestrictPackageTelemetry ? string.Empty : manifest.Version
                });
            }

            return packages;
        }
    }
}
