using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models
{
    public class MetricsConsentService : IMetricsConsentService
    {
        public ConsentLevel GetConsentLevel() => ConsentLevel.Detailed;
    }
}
