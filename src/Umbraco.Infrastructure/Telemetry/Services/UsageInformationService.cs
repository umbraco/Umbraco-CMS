using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Core.Services;

internal sealed class UsageInformationService : IUsageInformationService
{
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly IEnumerable<IDetailedTelemetryProvider> _providers;
    private readonly ILogger<UsageInformationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageInformationService"/> class.
    /// </summary>
    /// <param name="metricsConsentService">Service used to manage and check user consent for metrics collection.</param>
    /// <param name="providers">A collection of providers that supply detailed telemetry data.</param>
    /// <param name="logger">The logger.</param>
    public UsageInformationService(
        IMetricsConsentService metricsConsentService,
        IEnumerable<IDetailedTelemetryProvider> providers,
        ILogger<UsageInformationService> logger)
    {
        _metricsConsentService = metricsConsentService;
        _providers = providers;
        _logger = logger;
    }

    /// <summary>
    /// Returns detailed usage information collected from all registered telemetry providers, if the telemetry consent level is set to <see cref="TelemetryLevel.Detailed"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{UsageInformation}"/> containing detailed usage data, or <c>null</c> if detailed telemetry consent is not granted.
    /// </returns>
    /// <remarks>
    /// A provider that throws is skipped and logged, so one failing provider does not stop the rest of the report.
    /// </remarks>
    public IEnumerable<UsageInformation>? GetDetailed()
    {
        if (_metricsConsentService.GetConsentLevel() != TelemetryLevel.Detailed)
        {
            return null;
        }

        var detailedUsageInformation = new List<UsageInformation>();
        foreach (IDetailedTelemetryProvider provider in _providers)
        {
            try
            {
                // Materialize inside the try, so exceptions from lazily evaluated providers are caught here too.
                detailedUsageInformation.AddRange(provider.GetInformation().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Telemetry provider {TelemetryProvider} failed and was skipped.", provider.GetType().FullName);
            }
        }

        return detailedUsageInformation;
    }
}
