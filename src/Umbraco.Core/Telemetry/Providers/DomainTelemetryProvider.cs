using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class DomainTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IDomainService _domainService;

        public DomainTelemetryProvider(IDomainService domainService) => _domainService = domainService;

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();
            var domains = _domainService.GetAll(true).Count();

            result.Add(new UsageInformation("DomainCount", domains));
            return result;
        }
    }
}
