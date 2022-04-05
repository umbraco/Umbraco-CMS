using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public class UsageInformationService : IUsageInformationService
    {
        private readonly IMetricsConsentService _metricsConsentService;
        private readonly IDetailedTelemetryProvider[] _providers;

        public UsageInformationService(
            IMetricsConsentService metricsConsentService,
            IDetailedTelemetryProvider[] providers)
        {
            _metricsConsentService = metricsConsentService;
            _providers = providers;
        }

        public IEnumerable<UsageInformation> GetDetailed()
        {
            if (_metricsConsentService.GetConsentLevel() != ConsentLevel.Detailed)
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
}
