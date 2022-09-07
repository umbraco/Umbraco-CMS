using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Analytics;
using Umbraco.Cms.ManagementApi.ViewModels.Server;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

public class SetAnalyticsController : AnalyticsControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public SetAnalyticsController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public IActionResult SetConsentLevel(AnalyticsViewModel telemetryResource)
    {
        _metricsConsentService.SetConsentLevel(telemetryResource.TelemetryLevel);
        return Ok();
    }
}
