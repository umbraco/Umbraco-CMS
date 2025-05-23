using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IMetricsConsentService
{
    TelemetryLevel GetConsentLevel();

    Task SetConsentLevelAsync(TelemetryLevel telemetryLevel);
}
