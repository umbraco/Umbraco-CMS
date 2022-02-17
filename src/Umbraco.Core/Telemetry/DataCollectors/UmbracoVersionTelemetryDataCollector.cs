using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Telemetry.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects Umbraco version telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class UmbracoVersionTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IUmbracoVersion _umbracoVersion;

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.UmbracoVersion
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoVersionTelemetryDataCollector" /> class.
        /// </summary>
        public UmbracoVersionTelemetryDataCollector(IUmbracoVersion umbracoVersion) => _umbracoVersion = umbracoVersion;

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.UmbracoVersion => _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild(),
            _ => throw new NotSupportedException()
        };
    }
}
