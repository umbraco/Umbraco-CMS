using System.Collections.Generic;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <summary>
    /// Represents a telemetry data collector.
    /// </summary>
    public interface ITelemetryDataCollector
    {
        /// <summary>
        /// Gets the telemetry data this instance can collect.
        /// </summary>
        /// <value>
        /// The telemetry data this instance can collect.
        /// </value>
        IEnumerable<TelemetryData> Data { get; }

        /// <summary>
        /// Collects the specified telemetry data.
        /// </summary>
        /// <param name="telemetryData">The telemetry data.</param>
        /// <returns>
        /// The collected telemetry data.
        /// </returns>
        object Collect(TelemetryData telemetryData);
    }
}
