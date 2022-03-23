using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects telemetry metadata.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class MetadataTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly ISiteIdentifierService _siteIdentifierService;

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.TelemetryId,
            TelemetryData.Network
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataTelemetryDataCollector" /> class.
        /// </summary>
        public MetadataTelemetryDataCollector(ISiteIdentifierService siteIdentifierService) => _siteIdentifierService = siteIdentifierService;

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => s_data;

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.TelemetryId => GetTelemetryId(),
            TelemetryData.Network => true,
            _ => throw new NotSupportedException()
        };

        private Guid? GetTelemetryId()
            => _siteIdentifierService.TryGetSiteIdentifier(out var telemetryId) ? telemetryId : null;
    }
}
