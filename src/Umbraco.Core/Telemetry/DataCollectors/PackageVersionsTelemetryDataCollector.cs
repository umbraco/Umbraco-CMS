using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects package versions telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class PackageVersionsTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IManifestParser _manifestParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersionsTelemetryDataCollector" /> class.
        /// </summary>
        public PackageVersionsTelemetryDataCollector(IManifestParser manifestParser) => _manifestParser = manifestParser;

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => new[]
        {
            TelemetryData.PackageVersions
        };

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.PackageVersions => GetPackageVersions(),
            _ => throw new NotSupportedException()
        };

        private IEnumerable<PackageTelemetry> GetPackageVersions()
        {
            List<PackageTelemetry> packages = new();

            foreach (PackageManifest manifest in _manifestParser.GetManifests())
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
