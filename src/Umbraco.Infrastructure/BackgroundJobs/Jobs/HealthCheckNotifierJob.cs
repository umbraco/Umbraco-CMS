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

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Hosted service implementation for recurring health check notifications.
/// </summary>
public class HealthCheckNotifierJob : IRecurringBackgroundJob
{
    

    public TimeSpan Period { get; private set; }
    public TimeSpan Delay { get; private set; }

    private event EventHandler? _periodChanged;
    public event EventHandler PeriodChanged
    {
        add { _periodChanged += value; }
        remove { _periodChanged -= value; }
    }

    private readonly HealthCheckCollection _healthChecks;
    private readonly ILogger<HealthCheckNotifierJob> _logger;
    private readonly HealthCheckNotificationMethodCollection _notifications;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ICoreScopeProvider _scopeProvider;
    private HealthChecksSettings _healthChecksSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HealthCheckNotifierJob" /> class.
    /// </summary>
    /// <param name="healthChecksSettings">The configuration for health check settings.</param>
    /// <param name="healthChecks">The collection of healthchecks.</param>
    /// <param name="notifications">The collection of healthcheck notification methods.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    /// <param name="cronTabParser">Parser of crontab expressions.</param>
    public HealthCheckNotifierJob(
        IOptionsMonitor<HealthChecksSettings> healthChecksSettings,
        HealthCheckCollection healthChecks,
        HealthCheckNotificationMethodCollection notifications,
        ICoreScopeProvider scopeProvider,
        ILogger<HealthCheckNotifierJob> logger,
        IProfilingLogger profilingLogger,
        ICronTabParser cronTabParser)
    {
        _healthChecksSettings = healthChecksSettings.CurrentValue;
        _healthChecks = healthChecks;
        _notifications = notifications;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _profilingLogger = profilingLogger;

        Period = healthChecksSettings.CurrentValue.Notification.Period;
        Delay = DelayCalculator.GetDelay(healthChecksSettings.CurrentValue.Notification.FirstRunTime, cronTabParser, logger, TimeSpan.FromMinutes(3));


        healthChecksSettings.OnChange(x =>
        {
            _healthChecksSettings = x;
            Period = x.Notification.Period;
            _periodChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    public async Task RunJobAsync()
    {
        if (_healthChecksSettings.Notification.Enabled == false)
        {
            return;
        }

        // Ensure we use an explicit scope since we are running on a background thread and plugin health
        // checks can be making service/database calls so we want to ensure the CallContext/Ambient scope
        // isn't used since that can be problematic.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true))
        using (_profilingLogger.DebugDuration<HealthCheckNotifierJob>("Health checks executing", "Health checks complete"))
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
