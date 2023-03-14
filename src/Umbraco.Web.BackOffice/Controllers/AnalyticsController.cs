using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

public class AnalyticsController : UmbracoAuthorizedJsonController
{
    private readonly IMetricsConsentService _metricsConsentService;

    public AnalyticsController(IMetricsConsentService metricsConsentService) =>
        _metricsConsentService = metricsConsentService;

    public TelemetryLevel GetConsentLevel() => _metricsConsentService.GetConsentLevel();

    [HttpPost]
    public IActionResult SetConsentLevel([FromBody] TelemetryResource telemetryResource)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        _metricsConsentService.SetConsentLevel(telemetryResource.TelemetryLevel);
        return Ok();
    }

    public IEnumerable<TelemetryLevel> GetAllLevels() =>
        new[] { TelemetryLevel.Minimal, TelemetryLevel.Basic, TelemetryLevel.Detailed };
}
