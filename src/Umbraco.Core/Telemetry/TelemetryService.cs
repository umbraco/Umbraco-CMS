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
    /// <inheritdoc/>
    internal class TelemetryService : ITelemetryService
    {
        private readonly IOptionsMonitor<TelemetrySettings> _telemetrySettings;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IManifestParser _manifestParser;
        private readonly IUmbracoVersion _umbracoVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService" /> class.
        /// </summary>
        public TelemetryService(
            IOptionsMonitor<TelemetrySettings> telemetrySettings,
            IOptionsMonitor<GlobalSettings> globalSettings,
            IManifestParser manifestParser,
            IUmbracoVersion umbracoVersion)
        {
            _telemetrySettings = telemetrySettings;
            _globalSettings = globalSettings;
            _manifestParser = manifestParser;
            _umbracoVersion = umbracoVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService" /> class.
        /// </summary>
        [Obsolete("Use ctor with all params")]
        public TelemetryService(
            IOptionsMonitor<GlobalSettings> globalSettings,
            IManifestParser manifestParser,
            IUmbracoVersion umbracoVersion)
            : this(null, globalSettings, manifestParser, umbracoVersion)
        { }

        /// <inheritdoc/>
        public bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData)
        {
            TelemetrySettings telemetryOptions = _telemetrySettings?.CurrentValue;
            if (telemetryOptions == null)
            {
                // Added for backwards compatibility (remove in V10)
                telemetryOptions = new TelemetrySettings();
                telemetryOptions.Set(TelemetryLevel.Basic);
            }

            if (telemetryOptions.IsEnabled() is false ||
                TryGetTelemetryId(out Guid telemetryId) is false)
            {
                telemetryReportData = null;
                return false;
            }

            telemetryReportData = new TelemetryReportData
            {
                Id = telemetryId
            };

            if (telemetryOptions.IsEnabled(SystemInformationMetric.UmbracoVersion))
            {
                telemetryReportData.Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild();
            }

            if (telemetryOptions.IsEnabled(SystemInformationMetric.PackageVersions))
            {
                telemetryReportData.Packages = GetPackageTelemetry();
            }

            return true;
        }

        private bool TryGetTelemetryId(out Guid telemetryId)
        {
            // Parse telemetry string as a GUID & verify its a GUID and not some random string
            // since users may have messed with or decided to empty the app setting or put in something random
            if (Guid.TryParse(_globalSettings.CurrentValue.Id, out var parsedTelemetryId) is false)
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
                    Version = manifest.Version ?? string.Empty
                });
            }

            return packages;
        }
    }
}
