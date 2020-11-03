using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.Configuration.Extensions;
using Umbraco.Infrastructure.HealthCheck;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Hosted service implementation for recurring health check notifications.
    /// </summary>
    public class HealthCheckNotifier : RecurringHostedServiceBase
    {
        private readonly HealthChecksSettings _healthChecksSettings;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;
        private readonly IRuntimeState _runtimeState;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IMainDom _mainDom;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<HealthCheckNotifier> _logger;
        private readonly IProfilingLogger _profilingLogger;

        public HealthCheckNotifier(
            IOptions<HealthChecksSettings> healthChecksSettings,
            HealthCheckCollection healthChecks,
            HealthCheckNotificationMethodCollection notifications,
            IRuntimeState runtimeState,
            IServerRegistrar serverRegistrar,
            IMainDom mainDom,
            IScopeProvider scopeProvider,
            ILogger<HealthCheckNotifier> logger,
            IProfilingLogger profilingLogger,
            ICronTabParser cronTabParser)
            : base(healthChecksSettings.Value.Notification.Period,
                   healthChecksSettings.Value.GetNotificationDelay(cronTabParser, DateTime.Now, DefaultDelay))
        {
            _healthChecksSettings = healthChecksSettings.Value;
            _healthChecks = healthChecks;
            _notifications = notifications;
            _runtimeState = runtimeState;
            _serverRegistrar = serverRegistrar;
            _mainDom = mainDom;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _profilingLogger = profilingLogger;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            if (_healthChecksSettings.Notification.Enabled == false)
            {
                return;
            }

            if (_runtimeState.Level != RuntimeLevel.Run)
            {
                return;
            }

            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
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
            using (var scope = _scopeProvider.CreateScope())
            using (_profilingLogger.DebugDuration<HealthCheckNotifier>("Health checks executing", "Health checks complete"))
            {
                // Don't notify for any checks that are disabled, nor for any disabled just for notifications.
                var disabledCheckIds = _healthChecksSettings.Notification.DisabledChecks
                        .Select(x => x.Id)
                    .Union(_healthChecksSettings.DisabledChecks
                        .Select(x => x.Id))
                    .Distinct()
                    .ToArray();

                var checks = _healthChecks
                    .Where(x => disabledCheckIds.Contains(x.Id) == false);

                var results = new HealthCheckResults(checks);
                results.LogResults();

                // Send using registered notification methods that are enabled.
                foreach (var notificationMethod in _notifications.Where(x => x.Enabled))
                {
                    await notificationMethod.SendAsync(results);
                }
            }
        }
    }
}
