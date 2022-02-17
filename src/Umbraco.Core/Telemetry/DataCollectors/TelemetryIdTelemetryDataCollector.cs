using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects telemetry identifier telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class TelemetryIdTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.TelemetryId
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryIdTelemetryDataCollector" /> class.
        /// </summary>
        public TelemetryIdTelemetryDataCollector(IOptionsMonitor<GlobalSettings> globalSettings) => _globalSettings = globalSettings;

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.TelemetryId => GetTelemetryId(),
            _ => throw new NotSupportedException()
        };

        private Guid? GetTelemetryId()
            => Guid.TryParse(_globalSettings.CurrentValue.Id, out var telemetryId) ? telemetryId : null;
    }
}
