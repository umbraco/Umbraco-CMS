using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Server;

namespace Umbraco.Cms.ManagementApi.Controllers.Analytics;

[ApiVersion("1.0")]
public class GetAnalyticsController : AnalyticsControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public GetAnalyticsController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public TelemetryLevel Get() => _metricsConsentService.GetConsentLevel();
}
