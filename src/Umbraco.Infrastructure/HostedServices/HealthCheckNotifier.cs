// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Hosted service implementation for recurring health check notifications.
/// </summary>
public class HealthCheckNotifier : RecurringHostedServiceBase
{
    private readonly HealthCheckCollection _healthChecks;
    private readonly ILogger<HealthCheckNotifier> _logger;
    private readonly IMainDom _mainDom;
    private readonly HealthCheckNotificationMethodCollection _notifications;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IRuntimeState _runtimeState;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IServerRoleAccessor _serverRegistrar;
    private HealthChecksSettings _healthChecksSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckNotifier" /> class.
    /// </summary>
    /// <param name="healthChecksSettings">The configuration for health check settings.</param>
    /// <param name="healthChecks">The collection of healthchecks.</param>
    /// <param name="notifications">The collection of healthcheck notification methods.</param>
    /// <param name="runtimeState">Representation of the state of the Umbraco runtime.</param>
    /// <param name="serverRegistrar">Provider of server registrations to the distributed cache.</param>
    /// <param name="mainDom">Representation of the main application domain.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    /// <param name="cronTabParser">Parser of crontab expressions.</param>
    public HealthCheckNotifier(
        IOptionsMonitor<HealthChecksSettings> healthChecksSettings,
        HealthCheckCollection healthChecks,
        HealthCheckNotificationMethodCollection notifications,
        IRuntimeState runtimeState,
        IServerRoleAccessor serverRegistrar,
        IMainDom mainDom,
        ICoreScopeProvider scopeProvider,
        ILogger<HealthCheckNotifier> logger,
        IProfilingLogger profilingLogger,
        ICronTabParser cronTabParser)
        : base(
            logger,
            healthChecksSettings.CurrentValue.Notification.Period,
            GetDelay(healthChecksSettings.CurrentValue.Notification.FirstRunTime, cronTabParser, logger, DefaultDelay))
    {
        _healthChecksSettings = healthChecksSettings.CurrentValue;
        _healthChecks = healthChecks;
        _notifications = notifications;
        _runtimeState = runtimeState;
        _serverRegistrar = serverRegistrar;
        _mainDom = mainDom;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _profilingLogger = profilingLogger;

        healthChecksSettings.OnChange(x =>
        {
            _healthChecksSettings = x;
            ChangePeriod(x.Notification.Period);
        });
    }

    public override async Task PerformExecuteAsync(object? state)
    {
        if (_healthChecksSettings.Notification.Enabled == false)
        {
            return;
        }

        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        switch (_serverRegistrar.CurrentServerRole)
        {
            case ServerRole.Subscriber:
                _logger.LogDebug("Does not run on subscriber servers.");
                return;
            case ServerRole.Unknown:
                _logger.LogDebug("Does not run on servers with unknown role.");
                return;
        }

        // Ensure we do not run if not main domain, but do NOT lock it
        if (_mainDom.IsMainDom == false)
        {
            _logger.LogDebug("Does not run if not MainDom.");
            return;
        }

        // Ensure we use an explicit scope since we are running on a background thread and plugin health
        // checks can be making service/database calls so we want to ensure the CallContext/Ambient scope
        // isn't used since that can be problematic.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        using (_profilingLogger.DebugDuration<HealthCheckNotifier>("Health checks executing", "Health checks complete"))
        {
            // Don't notify for any checks that are disabled, nor for any disabled just for notifications.
            Guid[] disabledCheckIds = _healthChecksSettings.Notification.DisabledChecks
                .Select(x => x.Id)
                .Union(_healthChecksSettings.DisabledChecks
                    .Select(x => x.Id))
                .Distinct()
                .ToArray();

            IEnumerable<HealthCheck> checks = _healthChecks
                .Where(x => disabledCheckIds.Contains(x.Id) == false);

            HealthCheckResults results = await HealthCheckResults.Create(checks);
            results.LogResults();

            // Send using registered notification methods that are enabled.
            foreach (IHealthCheckNotificationMethod notificationMethod in _notifications.Where(x => x.Enabled))
            {
                await notificationMethod.SendAsync(results);
            }
        }
    }
}
