using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Core.Telemetry.Models
{
    /// <summary>
    /// Extension methods for <see cref="TelemetryLevel" />.
    /// </summary>
    public static class TelemetryLevelExtensions
    {
        /// <summary>
        /// Gets the telemetry data to collect for the specified level.
        /// </summary>
        /// <param name="level">The telemetry level.</param>
        /// <returns>
        /// The telemetry data to collect for the specified level.
        /// </returns>
        public static IEnumerable<TelemetryData> GetTelemetryData(this TelemetryLevel level) => level switch
        {
            TelemetryLevel.Off => Enumerable.Empty<TelemetryData>(),
            TelemetryLevel.Basic => new[]
            {
                TelemetryData.TelemetryId,
                TelemetryData.UmbracoVersion,
                TelemetryData.PackageVersions
            },
            TelemetryLevel.Detailed => Enum.GetValues(typeof(TelemetryData)).Cast<TelemetryData>(),
            _ => throw new NotSupportedException("The specified telemetry level is not supported.")
        };
    }
}
