using Umbraco.Core.Telemetry.Models;

namespace Umbraco.Core.Telemetry
{
    /// <summary>
    /// Service which gathers the data for telemetry reporting
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Try and get the <see cref="TelemetryReportData"/>
        /// </summary>
        bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData);
    }
}
