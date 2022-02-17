using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry
{
    /// <summary>
    /// Service which gathers the data for telemetry reporting.
    /// </summary>
    public interface ITelemetryService
    {
        /// <summary>
        /// Creates a new <see cref="TelemetryReportData" /> if telemetry can be collected.
        /// </summary>
        /// <param name="telemetryReportData">The telemetry report data.</param>
        /// <returns>
        ///   <c>true</c> if telemetry data could be collected; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetTelemetryReportData(out TelemetryReportData telemetryReportData);
    }
}
