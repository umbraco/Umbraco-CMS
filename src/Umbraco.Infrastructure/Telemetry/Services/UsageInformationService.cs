using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Core.Services;

internal sealed class UsageInformationService : IUsageInformationService
{
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly IEnumerable<IDetailedTelemetryProvider> _providers;

    public UsageInformationService(
        IMetricsConsentService metricsConsentService,
        IEnumerable<IDetailedTelemetryProvider> providers)
    {
        _metricsConsentService = metricsConsentService;
        _providers = providers;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<UsageInformation>?> GetDetailedAsync()
    {
        if (await _metricsConsentService.GetConsentLevelAsync() != TelemetryLevel.Detailed)
        {
            return null;
        }

        var detailedUsageInformation = new List<UsageInformation>();
        foreach (IDetailedTelemetryProvider provider in _providers)
        {
            detailedUsageInformation.AddRange(provider.GetInformation());
        }

        return detailedUsageInformation;
    }
}
