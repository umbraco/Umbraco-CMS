using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Infrastructure.Telemetry.Providers;

public class DomainTelemetryProvider : IDetailedTelemetryProvider
{
    private readonly IDomainService _domainService;

    public DomainTelemetryProvider(IDomainService domainService) => _domainService = domainService;

    public IEnumerable<UsageInformation> GetInformation()
    {
        var domains = _domainService.GetAll(true).Count();
        yield return new UsageInformation(Constants.Telemetry.DomainCount, domains);
    }
}
