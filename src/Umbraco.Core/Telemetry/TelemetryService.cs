using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <inheritdoc/>
    internal class TelemetryService : ITelemetryService
    {
        private readonly IOptionsMonitor<TelemetrySettings> _telemetrySettings;
        private readonly IEnumerable<ITelemetryDataCollector> _telemetryDataCollectors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService" /> class.
        /// </summary>
        public TelemetryService(
            IOptionsMonitor<TelemetrySettings> telemetrySettings,
            IEnumerable<ITelemetryDataCollector> telemetryDataCollectors)
        {
            _telemetrySettings = telemetrySettings;
            _telemetryDataCollectors = telemetryDataCollectors;
        }

        /// <inheritdoc/>
        public bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData)
        {
            TelemetrySettings telemetrySettings = _telemetrySettings.CurrentValue;
            if (telemetrySettings.IsEnabled() is false)
            {
                telemetryReportData = null;
                return false;
            }

            telemetryReportData = new TelemetryReportData();

            // Collect telemetry data
            foreach (var collector in _telemetryDataCollectors)
            {
                foreach (var telemetryData in collector.Data)
                {
                    if (telemetrySettings.IsEnabled(telemetryData))
                    {
                        telemetryReportData[telemetryData] = collector.Collect(telemetryData);
                    }
                }
            }

            return true;
        }
    }
}
