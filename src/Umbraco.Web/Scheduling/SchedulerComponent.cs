using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Scheduling
{
    internal sealed class SchedulerComponent : IComponent
    {
        private readonly IRuntimeState _runtime;
        private readonly IContentService _contentService;
        private readonly IAuditService _auditService;
        private readonly IProfilingLogger _logger;
        private readonly IScopeProvider _scopeProvider;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;

        private BackgroundTaskRunner<IBackgroundTask> _keepAliveRunner;
        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _tasksRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;
        private BackgroundTaskRunner<IBackgroundTask> _healthCheckRunner;

        private bool _started;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;

        public SchedulerComponent(IRuntimeState runtime,
            IContentService contentService, IAuditService auditService,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IScopeProvider scopeProvider, IProfilingLogger logger)
        {
            _runtime = runtime;
            _contentService = contentService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _logger = logger;

            _healthChecks = healthChecks;
            _notifications = notifications;
        }

        public void Initialize()
        {
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", _logger);
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", _logger);
            _tasksRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledTasks", _logger);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", _logger);
            _healthCheckRunner = new BackgroundTaskRunner<IBackgroundTask>("HealthCheckNotifier", _logger);

            // we will start the whole process when a successful request is made
            UmbracoModule.RouteAttempt += RegisterBackgroundTasksOnce;
        }

        public void Terminate()
        {
            // the AppDomain / maindom / whatever takes care of stopping background task runners
        }

        private void RegisterBackgroundTasksOnce(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                    UmbracoModule.RouteAttempt -= RegisterBackgroundTasksOnce;
                    RegisterBackgroundTasks();
                    break;
            }
        }

        private void RegisterBackgroundTasks()
        {
            LazyInitializer.EnsureInitialized(ref _tasks, ref _started, ref _locker, () =>
            {
                _logger.Debug<SchedulerComponent>("Initializing the scheduler");
                var settings = Current.Configs.Settings();

                var tasks = new List<IBackgroundTask>();

                tasks.Add(RegisterKeepAlive());
                tasks.Add(RegisterScheduledPublishing());
                tasks.Add(RegisterLogScrubber(settings));

                var healthCheckConfig = Current.Configs.HealthChecks();
                if (healthCheckConfig.NotificationSettings.Enabled)
                    tasks.Add(RegisterHealthCheckNotifier(healthCheckConfig, _healthChecks, _notifications, _logger));

                return tasks.ToArray();
            });
        }

        private IBackgroundTask RegisterKeepAlive()
        {
            // ping/keepalive
            // on all servers
            var task = new KeepAlive(_keepAliveRunner, 60000, 300000, _runtime, _logger);
            _keepAliveRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterScheduledPublishing()
        {
            // scheduled publishing/unpublishing
            // install on all, will only run on non-replica servers
            var task = new ScheduledPublishing(_publishingRunner, 60000, 60000, _runtime, _contentService, _logger);
            _publishingRunner.TryAdd(task);
            return task;
        }
        
        private IBackgroundTask RegisterHealthCheckNotifier(IHealthChecks healthCheckConfig,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IProfilingLogger logger)
        {
            // If first run time not set, start with just small delay after application start
            int delayInMilliseconds;
            if (string.IsNullOrEmpty(healthCheckConfig.NotificationSettings.FirstRunTime))
            {
                delayInMilliseconds = 60000;
            }
            else
            {
                // Otherwise start at scheduled time
                delayInMilliseconds = DateTime.Now.PeriodicMinutesFrom(healthCheckConfig.NotificationSettings.FirstRunTime) * 60 * 1000;
                if (delayInMilliseconds < 60000)
                {
                    delayInMilliseconds = 60000;
                }
            }

            var periodInMilliseconds = healthCheckConfig.NotificationSettings.PeriodInHours * 60 * 60 * 1000;
            var task = new HealthCheckNotifier(_healthCheckRunner, delayInMilliseconds, periodInMilliseconds, healthChecks, notifications, _runtime, logger);
            _healthCheckRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterLogScrubber(IUmbracoSettingsSection settings)
        {
            // log scrubbing
            // install on all, will only run on non-replica servers
            var task = new LogScrubber(_scrubberRunner, 60000, LogScrubber.GetLogScrubbingInterval(settings, _logger), _runtime, _auditService, settings, _scopeProvider, _logger);
            _scrubberRunner.TryAdd(task);
            return task;
        }
    }
}
