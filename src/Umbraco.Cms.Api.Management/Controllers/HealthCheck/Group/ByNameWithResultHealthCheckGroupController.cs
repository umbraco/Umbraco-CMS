using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

public class ByNameWithResultHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly IHealthCheckGroupWithResultViewModelFactory _healthCheckGroupWithResultViewModelFactory;

    public ByNameWithResultHealthCheckGroupController(
        HealthCheckCollection healthChecks,
        IHealthCheckGroupWithResultViewModelFactory healthCheckGroupWithResultViewModelFactory)
    {
        _healthChecks = healthChecks;
        _healthCheckGroupWithResultViewModelFactory = healthCheckGroupWithResultViewModelFactory;
    }

    /// <summary>
    ///     Gets a health check group with all its health checks by a group name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <remarks>The health check result(s) will be included as part of the health checks.</remarks>
    /// <returns>The health check group or not found result.</returns>
    [HttpGet("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HealthCheckGroupWithResultViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckGroupWithResultViewModel>> ByName(string name)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupWithResultViewModelFactory
            .CreateGroupingFromHealthCheckCollection(_healthChecks);

        IGrouping<string?, Core.HealthChecks.HealthCheck>? group = groups.FirstOrDefault(x => x.Key.InvariantEquals(name.Trim()));

        if (group is null)
        {
            return await Task.FromResult(NotFound());
        }

        return await Task.FromResult(Ok(_healthCheckGroupWithResultViewModelFactory.CreateHealthCheckGroupWithResultViewModel(group)));
    }
}
