// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Notifications;
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
    private readonly IEventAggregator _eventAggregator;
    private readonly HealthChecksSettings _healthChecksSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckController" /> class.
    /// </summary>
    [Obsolete("Use constructor that accepts IEventAggregator as a parameter, scheduled for removal in V14")]
    public HealthCheckController(HealthCheckCollection checks, ILogger<HealthCheckController> logger, IOptions<HealthChecksSettings> healthChecksSettings)
        : this(checks, logger, healthChecksSettings, StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>())
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckController" /> class.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public HealthCheckController(HealthCheckCollection checks, ILogger<HealthCheckController> logger, IOptions<HealthChecksSettings> healthChecksSettings, IEventAggregator eventAggregator)
    {
        _checks = checks ?? throw new ArgumentNullException(nameof(checks));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventAggregator = eventAggregator ?? throw new ArgumentException(nameof(eventAggregator));
        _healthChecksSettings = healthChecksSettings?.Value ?? throw new ArgumentException(nameof(healthChecksSettings));

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
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug("Running health check: " + check.Name);
            }

            if (!_healthChecksSettings.Notification.Enabled)
            {
                return await check.GetStatus();
            }

            HealthCheckResults results = await HealthCheckResults.Create(check);
            _eventAggregator.Publish(new HealthCheckCompletedNotification(results));


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
