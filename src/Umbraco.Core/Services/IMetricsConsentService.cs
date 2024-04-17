using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IMetricsConsentService
{
    TelemetryLevel GetConsentLevel();

    [Obsolete("Please use SetConsentLevelAsync instead, scheduled for removal in V15")]
    void SetConsentLevel(TelemetryLevel telemetryLevel);

    Task SetConsentLevelAsync(TelemetryLevel telemetryLevel);
}
