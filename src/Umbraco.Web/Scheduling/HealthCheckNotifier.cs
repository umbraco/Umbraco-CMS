using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Web.Scheduling
{
    internal class HealthCheckNotifier : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IHealthCheckResolver _healthCheckResolver;

        public HealthCheckNotifier(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _healthCheckResolver = HealthCheckResolver.Current;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_appContext == null) return true; // repeat...

            switch (_appContext.GetCurrentServerRole())
            {
                case ServerRole.Slave:
                    LogHelper.Debug<HealthCheckNotifier>("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    LogHelper.Debug<HealthCheckNotifier>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_appContext.MainDom.IsMainDom == false)
            {
                LogHelper.Debug<HealthCheckNotifier>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (_appContext.ProfilingLogger.DebugDuration<KeepAlive>("Health checks executing", "Health checks complete"))
            {
                var healthCheckConfig = UmbracoConfig.For.HealthCheck();

                // Don't notify for any checks that are disabled, nor for any disabled
                // just for notifications
                var disabledCheckIds = healthCheckConfig.NotificationSettings.DisabledChecks
                        .Select(x => x.Id)
                    .Union(healthCheckConfig.DisabledChecks
                        .Select(x => x.Id))
                    .Distinct()
                    .ToArray();

                var checks = _healthCheckResolver.HealthChecks
                    .Where(x => disabledCheckIds.Contains(x.Id) == false);

                var results = new HealthCheckResults(checks);
                results.LogResults();

                // Send using registered notification methods that are enabled
                var registeredNotificationMethods = HealthCheckNotificationMethodResolver.Current.NotificationMethods.Where(x => x.Enabled);
                foreach (var notificationMethod in registeredNotificationMethods)
                {
                    await notificationMethod.SendAsync(results);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync
        {
            get { return true; }
        }
    }
}
