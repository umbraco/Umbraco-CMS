using System.Collections.Generic;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Configuration settings for Umbraco telemetry data.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigTelemetry)]
    public class TelemetrySettings
    {
        /// <summary>
        /// Gets or sets a predifined telemetry level.
        /// </summary>
        /// <value>
        /// The telemetry level.
        /// </value>
        public TelemetryLevel? Level { get; set; }

        /// <summary>
        /// Gets the telemetry data to collect.
        /// </summary>
        /// <value>
        /// The telemetry data to collect.
        /// </value>
        public IDictionary<TelemetryData, bool> Data { get; } = new Dictionary<TelemetryData, bool>();
    }
}
