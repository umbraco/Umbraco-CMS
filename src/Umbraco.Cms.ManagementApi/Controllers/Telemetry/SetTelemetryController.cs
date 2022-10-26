using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Telemetry;

namespace Umbraco.Cms.ManagementApi.Controllers.Telemetry;

public class SetTelemetryController : TelemetryControllerBase
{
    private readonly IMetricsConsentService _metricsConsentService;

    public SetTelemetryController(IMetricsConsentService metricsConsentService) => _metricsConsentService = metricsConsentService;

    [HttpPost("level")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetConsentLevel(TelemetryLevelViewModel telemetryLevelViewModel)
    {
        if (!Enum.IsDefined(telemetryLevelViewModel.TelemetryLevel))
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

        _metricsConsentService.SetConsentLevel(telemetryLevelViewModel.TelemetryLevel);
        return await Task.FromResult(Ok());
    }
}
