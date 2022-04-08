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
        public IActionResult SetConsentLevel([FromBody]TelemetryResource telemetryResource)
        {
            if (telemetryResource.TelemetryLevel == "Minimal")
            {
                _metricsConsentService.SetConsentLevel(ConsentLevel.Minimal);
                return Ok();
            }

            if (telemetryResource.TelemetryLevel == "Basic")
            {
                _metricsConsentService.SetConsentLevel(ConsentLevel.Basic);
                return Ok();
            }

            if (telemetryResource.TelemetryLevel == "Detailed")
            {
                _metricsConsentService.SetConsentLevel(ConsentLevel.Detailed);
                return Ok();
            }

            return BadRequest();
        }

        public IEnumerable<ConsentLevel> GetAllLevels() => new[] { ConsentLevel.Minimal, ConsentLevel.Basic, ConsentLevel.Detailed };
    }
}
