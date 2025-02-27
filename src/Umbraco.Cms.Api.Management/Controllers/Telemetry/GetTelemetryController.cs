using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[ApiVersion("1.0")]
public class GetTelemetryController : TelemetryControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public GetTelemetryController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpGet("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TelemetryResponseModel), StatusCodes.Status200OK)]
    public async Task<TelemetryRepresentationBase> Get(CancellationToken cancellationToken)
        => await Task.FromResult(new TelemetryResponseModel { TelemetryLevel = _metricsConsentService.GetConsentLevel() });
}
