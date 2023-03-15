using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry;

/// <summary>
///     Service which gathers the data for telemetry reporting
/// </summary>
public interface ITelemetryService
{
    [Obsolete("Please use GetTelemetryReportDataAsync. Will be removed in V15.")]
    bool TryGetTelemetryReportData(out TelemetryReportData? telemetryReportData);

    /// <summary>
    ///     Attempts to get the <see cref="TelemetryReportData" />
    /// </summary>
    /// <remarks>
    ///     May return null if the site is in an unknown state.
    /// </remarks>
    Task<TelemetryReportData?> GetTelemetryReportDataAsync();
}
