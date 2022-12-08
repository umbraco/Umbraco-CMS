using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.HealthCheck;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

public class ByNameWithResultHealthCheckGroupController : HealthCheckGroupControllerBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly IHealthCheckGroupWithResultViewModelFactory _healthCheckGroupWithResultViewModelFactory;
    private readonly HealthChecksSettings _healthChecksSettings;

    public ByNameWithResultHealthCheckGroupController(
        HealthCheckCollection healthChecks,
        IHealthCheckGroupWithResultViewModelFactory healthCheckGroupWithResultViewModelFactory,
        IOptions<HealthChecksSettings> healthChecksSettings)
    {
        _healthChecks = healthChecks;
        _healthCheckGroupWithResultViewModelFactory = healthCheckGroupWithResultViewModelFactory;
        _healthChecksSettings = healthChecksSettings.Value;
    }

    /// <summary>
    ///     Gets a health check group with all its health checks by a group name.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <remarks>The health check result(s) will be included as part of the health checks.</remarks>
    /// <returns>The health check group or not found result.</returns>
    [HttpGet("{groupName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HealthCheckGroupWithResultViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthCheckGroupWithResultViewModel?>> ByName(string groupName)
    {
        IList<Guid> disabledCheckIds = _healthChecksSettings.DisabledChecks
            .Select(x => x.Id)
            .ToList();

        IGrouping<string?, Core.HealthChecks.HealthCheck>? group = _healthChecks
            .Where(x => disabledCheckIds.Contains(x.Id) == false)
            .GroupBy(x => x.Group)
            .FirstOrDefault(x => x.Key.InvariantEquals(groupName.Trim()));

        if (group is null)
        {
            return NotFound();
        }

        return await Task.FromResult(_healthCheckGroupWithResultViewModelFactory.CreateHealthCheckGroupWithResultViewModel(group));
    }
}
