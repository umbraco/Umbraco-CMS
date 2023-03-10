using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.HealthCheck;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

public class AllHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly IHealthCheckGroupViewModelFactory _healthCheckGroupViewModelFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllHealthCheckGroupController(
        IHealthCheckGroupViewModelFactory healthCheckGroupViewModelFactory,
        IUmbracoMapper umbracoMapper)
    {
        _healthCheckGroupViewModelFactory = healthCheckGroupViewModelFactory;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated grouped list of all names the health checks are grouped by.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of health checks group names.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<HealthCheckGroupResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<HealthCheckGroupResponseModel>>> All(int skip = 0, int take = 100)
    {
        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthCheckGroupViewModelFactory
            .CreateGroupingFromHealthCheckCollection()
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<HealthCheckGroupResponseModel>>(groups)));
    }
}
