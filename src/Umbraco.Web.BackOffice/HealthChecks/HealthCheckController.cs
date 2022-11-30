// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.HealthChecks;

/// <summary>
///     The API controller used to display the health check info and execute any actions
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class HealthCheckController : UmbracoAuthorizedJsonController
{
    private readonly HealthCheckCollection _checks;
    private readonly IList<Guid> _disabledCheckIds;
    private readonly ILogger<HealthCheckController> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckController" /> class.
    /// </summary>
    public HealthCheckController(HealthCheckCollection checks, ILogger<HealthCheckController> logger, IOptions<HealthChecksSettings> healthChecksSettings)
    {
        _checks = checks ?? throw new ArgumentNullException(nameof(checks));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        HealthChecksSettings healthCheckConfig =
            healthChecksSettings.Value ?? throw new ArgumentNullException(nameof(healthChecksSettings));
        _disabledCheckIds = healthCheckConfig.DisabledChecks
            .Select(x => x.Id)
            .ToList();
    }

    /// <summary>
    ///     Gets a grouped list of health checks, but doesn't actively check the status of each health check.
    /// </summary>
    /// <returns>Returns a collection of anonymous objects representing each group.</returns>
    public object GetAllHealthChecks()
    {
        IOrderedEnumerable<IGrouping<string?, HealthCheck>> groups = _checks
            .Where(x => _disabledCheckIds.Contains(x.Id) == false)
            .GroupBy(x => x.Group)
            .OrderBy(x => x.Key);
        var healthCheckGroups = new List<HealthCheckGroup>();
        foreach (IGrouping<string?, HealthCheck> healthCheckGroup in groups)
        {
            var hcGroup = new HealthCheckGroup
            {
                Name = healthCheckGroup.Key,
                Checks = healthCheckGroup
                    .OrderBy(x => x.Name)
                    .ToList()
            };
            healthCheckGroups.Add(hcGroup);
        }

        return healthCheckGroups;
    }

    /// <summary>
    ///     Gets the status of the HealthCheck with the specified id.
    /// </summary>
    [HttpGet]
    public async Task<object> GetStatus(Guid id)
    {
        HealthCheck check = GetCheckById(id);

        try
        {
            _logger.LogDebug("Running health check: " + check.Name);
            return await check.GetStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in health check: {HealthCheckName}", check.Name);
            throw;
        }
    }

    /// <summary>
    ///     Executes a given action from a HealthCheck.
    /// </summary>
    [HttpPost]
    public HealthCheckStatus ExecuteAction(HealthCheckAction action)
    {
        HealthCheck check = GetCheckById(action.HealthCheckId);
        return check.ExecuteAction(action);
    }

    private HealthCheck GetCheckById(Guid? id)
    {
        HealthCheck? check = _checks
            .Where(x => _disabledCheckIds.Contains(x.Id) == false)
            .FirstOrDefault(x => x.Id == id);

        if (check == null)
        {
            throw new InvalidOperationException($"No health check found with id {id}");
        }

        return check;
    }
}
