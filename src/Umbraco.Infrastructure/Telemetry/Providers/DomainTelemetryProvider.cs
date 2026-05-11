using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

/// <summary>
/// Provides telemetry data and insights related to domain configurations in the application.
/// </summary>
public class DomainTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IDomainService _domainService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainTelemetryProvider"/> class.
    /// </summary>
    /// <param name="domainService">The <see cref="IDomainService"/> used by this telemetry provider.</param>
    public DomainTelemetryProvider(IDomainService domainService) => _domainService = domainService;

    /// <summary>
    /// Gets usage information related to domains.
    /// </summary>
    /// <returns>An enumerable of <see cref="Umbraco.Cms.Infrastructure.Telemetry.UsageInformation"/> containing domain usage data.</returns>
    public IEnumerable<UsageInformation> GetInformation()
    {
        var domains = _domainService.GetAll(true).Count();
        yield return new UsageInformation(Constants.Telemetry.DomainCount, domains);
    }
}
