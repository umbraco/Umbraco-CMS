using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

public class CheckHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly IHealthCheckGroupPresentationFactory _healthCheckGroupPresentationFactory;

    public CheckHealthCheckGroupController(IHealthCheckGroupPresentationFactory healthCheckGroupPresentationFactory)
        => _healthCheckGroupPresentationFactory = healthCheckGroupPresentationFactory;

    /// <summary>
    ///     Check all health checks in the group with a given group name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <remarks>The health check result(s) will be included as part of the health checks.</remarks>
    /// <returns>The health check group or not found result.</returns>
    [HttpPost("{name}/check")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HealthCheckGroupWithResultResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckGroupWithResultResponseModel>> ByNameWithResult(string name)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupPresentationFactory
            .CreateGroupingFromHealthCheckCollection();

        IGrouping<string?, Core.HealthChecks.HealthCheck>? group = groups.FirstOrDefault(x => x.Key.InvariantEquals(name.Trim()));

        if (group is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(_healthCheckGroupPresentationFactory.CreateHealthCheckGroupWithResultViewModel(group)));
    }
}
