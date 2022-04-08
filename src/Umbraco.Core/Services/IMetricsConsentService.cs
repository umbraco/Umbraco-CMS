using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface IMetricsConsentService
    {
        public ConsentLevel GetConsentLevel();

        public void SetConsentLevel(ConsentLevel consentLevel);
    }
}
