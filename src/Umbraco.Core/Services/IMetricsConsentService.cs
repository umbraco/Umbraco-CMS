using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides operations for managing telemetry metrics consent.
/// </summary>
public interface IMetricsConsentService
{
    /// <summary>
    ///     Gets the current telemetry consent level.
    /// </summary>
    /// <returns>The current <see cref="TelemetryLevel"/> configured for the system.</returns>
    TelemetryLevel GetConsentLevel();

    /// <summary>
    ///     Sets the telemetry consent level asynchronously.
    /// </summary>
    /// <param name="telemetryLevel">The <see cref="TelemetryLevel"/> to set.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetConsentLevelAsync(TelemetryLevel telemetryLevel);
}
