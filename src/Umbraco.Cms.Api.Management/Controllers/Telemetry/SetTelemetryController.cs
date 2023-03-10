﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Telemetry;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

public class SetTelemetryController : TelemetryControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public SetTelemetryController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpPost("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetConsentLevel(TelemetryRequestModel telemetryRepresentationBase)
    {
        if (!Enum.IsDefined(telemetryRepresentationBase.TelemetryLevel))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Invalid TelemetryLevel value",
                Detail = "The provided value for TelemetryLevel is not valid",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return BadRequest(invalidModelProblem);
        }

        _metricsConsentService.SetConsentLevel(telemetryRepresentationBase.TelemetryLevel);
        return await Task.FromResult(Ok());
    }
}
