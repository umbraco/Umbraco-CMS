using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Core.Services;

internal sealed class UsageInformationService : IUsageInformationService
{
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly IEnumerable<IDetailedTelemetryProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageInformationService"/> class.
    /// </summary>
    /// <param name="metricsConsentService">Service used to manage and check user consent for metrics collection.</param>
    /// <param name="providers">A collection of providers that supply detailed telemetry data.</param>
    public UsageInformationService(
        IMetricsConsentService metricsConsentService,
        IEnumerable<IDetailedTelemetryProvider> providers)
    {
        _metricsConsentService = metricsConsentService;
        _providers = providers;
    }

    /// <summary>
    /// Returns detailed usage information collected from all registered telemetry providers, if the telemetry consent level is set to <see cref="TelemetryLevel.Detailed"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{UsageInformation}"/> containing detailed usage data, or <c>null</c> if detailed telemetry consent is not granted.
    /// </returns>
    public IEnumerable<UsageInformation>? GetDetailed()
    {
        if (_metricsConsentService.GetConsentLevel() != TelemetryLevel.Detailed)
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
