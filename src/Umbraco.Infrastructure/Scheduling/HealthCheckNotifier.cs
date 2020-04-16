using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Web.Scheduling
{
    public class HealthCheckNotifier : RecurringTaskBase
    {
        private readonly IMainDom _mainDom;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;
        private readonly IProfilingLogger _logger;
        private readonly IHealthChecksSettings _healthChecksSettingsConfig;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IRuntimeState _runtimeState;

        public HealthCheckNotifier(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IMainDom mainDom, IProfilingLogger logger, IHealthChecksSettings healthChecksSettingsConfig, IServerRegistrar serverRegistrar,
            IRuntimeState runtimeState)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _healthChecks = healthChecks;
            _notifications = notifications;
            _mainDom = mainDom;
            _logger = logger;
            _healthChecksSettingsConfig = healthChecksSettingsConfig;
            _serverRegistrar = serverRegistrar;
            _runtimeState = runtimeState;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return true; // repeat...

            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.Debug<HealthCheckNotifier>("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.Debug<HealthCheckNotifier>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.Debug<HealthCheckNotifier>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (_logger.DebugDuration<HealthCheckNotifier>("Health checks executing", "Health checks complete"))
            {
                var healthCheckConfig = _healthChecksSettingsConfig;

                // Don't notify for any checks that are disabled, nor for any disabled
                // just for notifications
                var disabledCheckIds = healthCheckConfig.NotificationSettings.DisabledChecks
                        .Select(x => x.Id)
                    .Union(healthCheckConfig.DisabledChecks
                        .Select(x => x.Id))
                    .Distinct()
                    .ToArray();

                var checks = _healthChecks
                    .Where(x => disabledCheckIds.Contains(x.Id) == false);

                var results = new HealthCheckResults(checks);
                results.LogResults();

                // Send using registered notification methods that are enabled
                foreach (var notificationMethod in _notifications.Where(x => x.Enabled))
                    await notificationMethod.SendAsync(results, token);
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
