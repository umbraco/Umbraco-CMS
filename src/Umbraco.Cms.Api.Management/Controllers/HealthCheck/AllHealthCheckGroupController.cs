using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

public class AllHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly HealthChecksSettings _healthChecksSettings;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllHealthCheckGroupController(
        HealthCheckCollection healthChecks,
        IOptions<HealthChecksSettings> healthChecksSettings,
        IUmbracoMapper umbracoMapper)
    {
        _healthChecks = healthChecks;
        _healthChecksSettings = healthChecksSettings.Value;
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
    public async Task<ActionResult<PagedViewModel<HealthCheckGroupViewModel>>> All(int skip, int take)
    {
        IList<Guid> disabledCheckIds = _healthChecksSettings.DisabledChecks
            .Select(x => x.Id)
            .ToList();

        IEnumerable<IGrouping<string?, Core.HealthChecks.HealthCheck>> groups = _healthChecks
            .Where(x => disabledCheckIds.Contains(x.Id) == false)
            .GroupBy(x => x.Group)
            .OrderBy(x => x.Key)
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(_umbracoMapper.Map<PagedViewModel<HealthCheckGroupViewModel>>(groups)!);
    }
}
