using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

public class ByNameHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly IHealthCheckGroupViewModelFactory _healthCheckGroupViewModelFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByNameHealthCheckGroupController(
        IHealthCheckGroupViewModelFactory healthCheckGroupViewModelFactory,
        IUmbracoMapper umbracoMapper)
    {
        _healthCheckGroupViewModelFactory = healthCheckGroupViewModelFactory;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a health check group with all its health checks by a group name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The health check group or not found result.</returns>
    [HttpGet("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HealthCheckGroupViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckGroupViewModel>> ByName(string name)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupViewModelFactory
            .CreateGroupingFromHealthCheckCollection();

        IGrouping<string?, Core.HealthChecks.HealthCheck>? group = groups.FirstOrDefault(x => x.Key.InvariantEquals(name.Trim()));

        if (group is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<HealthCheckGroupViewModel>(group)));
    }
}
