using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <inheritdoc/>
    internal class TelemetryService : ITelemetryService
    {
        private readonly IManifestParser _manifestParser;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ISiteIdentifierService _siteIdentifierService;
        private readonly UsageInformationService _usageInformationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        public TelemetryService(
            IManifestParser manifestParser,
            IUmbracoVersion umbracoVersion,
            ISiteIdentifierService siteIdentifierService, UsageInformationService usageInformationService)
        {
            _manifestParser = manifestParser;
            _umbracoVersion = umbracoVersion;
            _siteIdentifierService = siteIdentifierService;
            _usageInformationService = usageInformationService;
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
                Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
                Packages = GetPackageTelemetry(),
                Detailed = _usageInformationService.GetDetailed(),
            };
            return true;
        }

        private IEnumerable<PackageTelemetry> GetPackageTelemetry()
        {
            List<PackageTelemetry> packages = new();
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
                    Version = manifest.Version ?? string.Empty,
                });
            }

            return packages;
        }
    }
}
