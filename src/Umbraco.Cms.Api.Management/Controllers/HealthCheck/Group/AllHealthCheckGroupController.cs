using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

public class AllHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly IHealthCheckGroupWithResultViewModelFactory _healthCheckGroupWithResultViewModelFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllHealthCheckGroupController(
        HealthCheckCollection healthChecks,
        IHealthCheckGroupWithResultViewModelFactory healthCheckGroupWithResultViewModelFactory,
        IUmbracoMapper umbracoMapper)
    {
        _healthChecks = healthChecks;
        _healthCheckGroupWithResultViewModelFactory = healthCheckGroupWithResultViewModelFactory;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated grouped list of all health checks without checking the result of each health check.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of health checks, grouped by health check group name.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<HealthCheckGroupViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<HealthCheckGroupViewModel>>> All(int skip = 0, int take = 100)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupWithResultViewModelFactory
            .CreateGroupingFromHealthCheckCollection(_healthChecks)
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<HealthCheckGroupViewModel>>(groups)));
    }
}
