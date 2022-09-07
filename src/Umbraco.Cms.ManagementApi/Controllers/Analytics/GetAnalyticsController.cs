using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Analytics;
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
    public AnalyticsLevelViewModel Get() => new() { AnalyticsLevel = _metricsConsentService.GetConsentLevel() };
}
