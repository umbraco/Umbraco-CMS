using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IMetricsConsentService
{
    TelemetryLevel GetConsentLevel();

    void SetConsentLevel(TelemetryLevel telemetryLevel);
}
