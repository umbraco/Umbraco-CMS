using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

[ApiVersion("1.0")]
public class ByNameHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly IHealthCheckGroupPresentationFactory _healthCheckGroupPresentationFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByNameHealthCheckGroupController(
        IHealthCheckGroupPresentationFactory healthCheckGroupPresentationFactory,
        IUmbracoMapper umbracoMapper)
    {
        _healthCheckGroupPresentationFactory = healthCheckGroupPresentationFactory;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a health check group with all its health checks by a group name.
    /// </summary>
    /// <param name="name">The name of the group.</param>
    /// <returns>The health check group or not found result.</returns>
    [HttpGet("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HealthCheckGroupPresentationModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByName(
        CancellationToken cancellationToken,
        string name)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupPresentationFactory
            .CreateGroupingFromHealthCheckCollection();

        IGrouping<string?, Core.HealthChecks.HealthCheck>? group = groups.FirstOrDefault(x => x.Key.InvariantEquals(name.Trim()));

        if (group is null)
        {
            return HealthCheckGroupNotFound();
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<HealthCheckGroupPresentationModel>(group)));
    }
}
