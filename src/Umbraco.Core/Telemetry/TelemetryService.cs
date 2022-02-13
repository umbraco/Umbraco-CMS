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
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly IEnumerable<ITelemetryDataCollector> _telemetryDataCollectors;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService" /> class.
        /// </summary>
        public TelemetryService(
            IOptionsMonitor<TelemetrySettings> telemetrySettings,
            IOptionsMonitor<GlobalSettings> globalSettings,
            IEnumerable<ITelemetryDataCollector> telemetryDataCollectors)
        {
            _telemetrySettings = telemetrySettings;
            _globalSettings = globalSettings;
            _telemetryDataCollectors = telemetryDataCollectors;
        }

        /// <inheritdoc/>
        public bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData)
        {
            TelemetrySettings telemetrySettings = _telemetrySettings.CurrentValue;

            if (telemetrySettings.IsEnabled() is false ||
                TryGetTelemetryId(out Guid telemetryId) is false)
            {
                telemetryReportData = null;
                return false;
            }

            telemetryReportData = new TelemetryReportData
            {
                Id = telemetryId
            };

            // Collect telemetry data
            foreach (var collector in _telemetryDataCollectors)
            {
                foreach (var telemetryData in collector.Data)
                {
                    if (telemetrySettings.IsEnabled(telemetryData))
                    {
                        telemetryReportData.Data[telemetryData] = collector.Collect(telemetryData);
                    }
                }
            }

            // Populate existing fields for backwards compatibility
            if (telemetryReportData.Data.TryGetValue(TelemetryData.UmbracoVersion, out var umbracoVersion))
            {
                telemetryReportData.Version = umbracoVersion as string;
            }

            if (telemetryReportData.Data.TryGetValue(TelemetryData.PackageVersions, out var packageVersions))
            {
                telemetryReportData.Packages = packageVersions as IEnumerable<PackageTelemetry>;
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
    }
}
