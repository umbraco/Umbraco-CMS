using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Telemetry;

namespace Umbraco.Cms.ManagementApi.Controllers.Telemetry;

public class GetTelemetryController : TelemetryControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public GetTelemetryController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpGet("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TelemetryLevelViewModel), StatusCodes.Status200OK)]
    public async Task<TelemetryLevelViewModel> Get() => await Task.FromResult(new TelemetryLevelViewModel { TelemetryLevel = _metricsConsentService.GetConsentLevel() });
}
