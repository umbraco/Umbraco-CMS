using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Models
{
    public class MetricsConsentService : IMetricsConsentService
    {
        private ConsentLevel _consentLevel = ConsentLevel.Detailed;
        public ConsentLevel GetConsentLevel() => _consentLevel;
        public ConsentLevel SetConsentLevel(ConsentLevel consentLevel) => _consentLevel = consentLevel;
    }
}
