using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

[ApiVersion("1.0")]
public class ExecuteActionHealthCheckController : HealthCheckControllerBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IList<Guid> _disabledCheckIds;

    public ExecuteActionHealthCheckController(
        HealthCheckCollection healthChecks,
        IOptions<HealthChecksSettings> healthChecksSettings,
        IUmbracoMapper umbracoMapper)
    {
        _healthChecks = healthChecks;
        _disabledCheckIds = healthChecksSettings.Value
            .DisabledChecks
            .Select(x => x.Id)
            .ToList();
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Executes a given action from a HealthCheck.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result of a health check after the health check action is performed.</returns>
    [HttpPost("execute-action")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HealthCheckResultResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckResultResponseModel>> ExecuteAction(
        CancellationToken cancellationToken,
        HealthCheckActionRequestModel action)
    {
        Guid healthCheckKey = action.HealthCheck.Id;

        Core.HealthChecks.HealthCheck? healthCheck = _healthChecks
            .Where(x => _disabledCheckIds.Contains(healthCheckKey) == false)
            .FirstOrDefault(x => x.Id == healthCheckKey);

        if (healthCheck is null)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Health Check Not Found",
                Detail = $"No health check found with id = {healthCheckKey}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return BadRequest(invalidModelProblem);
        }

        HealthCheckStatus result = await healthCheck.ExecuteActionAsync(_umbracoMapper.Map<HealthCheckAction>(action)!);

        return await Task.FromResult(Ok(_umbracoMapper.Map<HealthCheckResultResponseModel>(result)));
    }
}
