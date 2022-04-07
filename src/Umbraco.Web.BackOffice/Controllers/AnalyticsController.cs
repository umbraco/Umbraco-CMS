using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public ConsentLevel SetConsentLevel(string consentLevel)
        {
            if (consentLevel == "Minimal")
            {
                return _metricsConsentService.SetConsentLevel(ConsentLevel.Minimal);
            }

            if (consentLevel == "Basic")
            {
                return _metricsConsentService.SetConsentLevel(ConsentLevel.Basic);
            }

            if (consentLevel == "Detailed")
            {
                return _metricsConsentService.SetConsentLevel(ConsentLevel.Detailed);
            }

            return ConsentLevel.Minimal;
        }

        public IEnumerable<ConsentLevel> GetAllLevels() => new[] { ConsentLevel.Minimal, ConsentLevel.Basic, ConsentLevel.Detailed };
    }
}
