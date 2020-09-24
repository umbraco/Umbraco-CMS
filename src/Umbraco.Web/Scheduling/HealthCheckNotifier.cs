using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Web.Scheduling
{
    internal class HealthCheckNotifier : RecurringTaskBase
    {
        private readonly IRuntimeState _runtimeState;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;
        private readonly IScopeProvider _scopeProvider;
        private readonly IProfilingLogger _logger;

        public HealthCheckNotifier(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IScopeProvider scopeProvider, IRuntimeState runtimeState, IProfilingLogger logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _healthChecks = healthChecks;
            _notifications = notifications;
            _scopeProvider = scopeProvider;
            _runtimeState = runtimeState;
            _logger = logger;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_runtimeState.Level != RuntimeLevel.Run)
                return true; // repeat...

            switch (_runtimeState.ServerRole)
            {
                case ServerRole.Replica:
                    _logger.Debug<HealthCheckNotifier>("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.Debug<HealthCheckNotifier>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_runtimeState.IsMainDom == false)
            {
                _logger.Debug<HealthCheckNotifier>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            // Ensure we use an explicit scope since we are running on a background thread and plugin health
            // checks can be making service/database calls so we want to ensure the CallContext/Ambient scope
            // isn't used since that can be problematic.
            using (var scope = _scopeProvider.CreateScope())
            using (_logger.DebugDuration<HealthCheckNotifier>("Health checks executing", "Health checks complete"))
            {
                var healthCheckConfig = Current.Configs.HealthChecks();

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
