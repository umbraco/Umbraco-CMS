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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetConsentLevel(AnalyticsLevelViewModel analyticsLevelViewModel)
    {
        if (!Enum.IsDefined(analyticsLevelViewModel.AnalyticsLevel))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Invalid AnalyticsLevel value",
                Detail = "The provided value for AnalyticsLevel is not valid",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return BadRequest(invalidModelProblem);
        }

        _metricsConsentService.SetConsentLevel(analyticsLevelViewModel.AnalyticsLevel);
        return await Task.FromResult(Ok());
    }
}
