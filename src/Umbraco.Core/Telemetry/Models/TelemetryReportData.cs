using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Serializable class containing telemetry information.
    /// </summary>
    public class TelemetryReportData : Dictionary<TelemetryData, object>
    {
        /// <summary>
        /// Gets or sets a random GUID to prevent an instance posting multiple times pr. day.
        /// </summary>
        [Obsolete("Get or set the ID from the dictionary using TelemetryData.TelemetryId instead.")]
        public Guid Id
        {
            get => TryGetValue(TelemetryData.TelemetryId, out var value) && value is Guid telemetryId ? telemetryId : default;
            set => this[TelemetryData.TelemetryId] = value;
        }

        /// <summary>
        /// Gets or sets the Umbraco CMS version.
        /// </summary>
        [Obsolete("Get or set the version from the dictionary using TelemetryData.UmbracoVersion instead.")]
        public string Version
        {
            get => TryGetValue(TelemetryData.UmbracoVersion, out var value) && value is string umbracoVersion ? umbracoVersion : default;
            set => this[TelemetryData.UmbracoVersion] = value;
        }

        /// <summary>
        /// Gets or sets an enumerable containing information about packages.
        /// </summary>
        /// <remarks>
        /// Contains only the name and version of the packages, unless no version is specified.
        /// </remarks>
        [Obsolete("Get or set the package versions from the dictionary using TelemetryData.PackageVersions instead.")]
        public IEnumerable<PackageTelemetry> Packages
        {
            get => TryGetValue(TelemetryData.PackageVersions, out var value) && value is IEnumerable<PackageTelemetry> packageVersions ? packageVersions : default;
            set => this[TelemetryData.PackageVersions] = value;
        }
    }
}
