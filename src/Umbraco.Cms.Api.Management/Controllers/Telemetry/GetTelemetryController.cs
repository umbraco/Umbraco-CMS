using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

/// <summary>
/// Controller responsible for handling requests related to retrieving telemetry data.
/// </summary>
[ApiVersion("1.0")]
public class GetTelemetryController : TelemetryControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTelemetryController"/> class.
    /// </summary>
    /// <param name="metricsConsentService">Service used to manage metrics consent.</param>
    public GetTelemetryController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    /// <summary>
    /// Retrieves the current telemetry configuration, including the user's consent level.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, with the telemetry information as its result.</returns>
    [HttpGet("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(TelemetryResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets telemetry information.")]
    [EndpointDescription("Gets the current telemetry configuration and consent level.")]
    public Task<TelemetryRepresentationBase> Get(CancellationToken cancellationToken)
        => Task.FromResult<TelemetryRepresentationBase>(new TelemetryResponseModel { TelemetryLevel = _metricsConsentService.GetConsentLevel() });
}
