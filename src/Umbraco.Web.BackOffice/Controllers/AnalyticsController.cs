using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    public class AnalyticsController : UmbracoAuthorizedJsonController
    {
        private readonly IMetricsConsentService _metricsConsentService;
        public AnalyticsController(IMetricsConsentService metricsConsentService)
        {
            _metricsConsentService = metricsConsentService;
        }

        public ConsentLevel GetConsentLevel()
        {
            return _metricsConsentService.GetConsentLevel();
        }

        public ConsentLevel SetConsentLevel(ConsentLevel consentLevel)
        {
            return _metricsConsentService.SetConsentLevel(consentLevel);
        }

        public IEnumerable<ConsentLevel> GetAllLevels() => new[] { ConsentLevel.Minimal, ConsentLevel.Basic, ConsentLevel.Detailed };
    }
}
