using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry;

/// <summary>
///     Service which gathers the data for telemetry reporting
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    ///     Attempts to get the <see cref="TelemetryReportData" />
    /// </summary>
    /// <remarks>
    ///     May return null if the site is in an unknown state.
    /// </remarks>
    Task<TelemetryReportData?> GetTelemetryReportDataAsync();
}
