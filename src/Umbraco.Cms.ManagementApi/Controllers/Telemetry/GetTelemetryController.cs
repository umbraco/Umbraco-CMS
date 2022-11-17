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
    [ProducesResponseType(typeof(TelemetryViewModel), StatusCodes.Status200OK)]
    public async Task<TelemetryViewModel> Get() => await Task.FromResult(new TelemetryViewModel { TelemetryLevel = _metricsConsentService.GetConsentLevel() });
}
