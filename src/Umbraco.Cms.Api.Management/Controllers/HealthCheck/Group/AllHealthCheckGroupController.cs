using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

[ApiVersion("1.0")]
public class AllHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly IHealthCheckGroupPresentationFactory _healthCheckGroupPresentationFactory;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllHealthCheckGroupController(
        IHealthCheckGroupPresentationFactory healthCheckGroupPresentationFactory,
        IUmbracoMapper umbracoMapper)
    {
        _healthCheckGroupPresentationFactory = healthCheckGroupPresentationFactory;
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
    public Task<ActionResult<PagedViewModel<HealthCheckGroupResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IGrouping<string?, Core.HealthChecks.HealthCheck>[] groups = _healthCheckGroupPresentationFactory
            .CreateGroupingFromHealthCheckCollection()
            .ToArray();

        var viewModel = new PagedViewModel<HealthCheckGroupResponseModel>
        {
            Total = groups.Length,
            Items = _umbracoMapper.MapEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>, HealthCheckGroupResponseModel>(groups.Skip(skip).Take(take))
        };

        return Task.FromResult<ActionResult<PagedViewModel<HealthCheckGroupResponseModel>>>(Ok(viewModel));
    }
}
