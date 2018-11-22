using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Components;
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
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// All tasks are run in a background task runner which is web aware and will wind down
    /// the task correctly instead of killing it completely when the app domain shuts down.
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    internal sealed class SchedulerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IRuntimeState _runtime;
        private IContentService _contentService;
        private IAuditService _auditService;
        private ILogger _logger;
        private ProfilingLogger _proflog;
        private IScopeProvider _scopeProvider;
        private HealthCheckCollection _healthChecks;
        private HealthCheckNotificationMethodCollection _notifications;

        private BackgroundTaskRunner<IBackgroundTask> _keepAliveRunner;
        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _tasksRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;
        private BackgroundTaskRunner<IBackgroundTask> _healthCheckRunner;

        private bool _started;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;
        private IContentPublishingService _contentPublishingService;

        public void Initialize(IRuntimeState runtime,
            IContentService contentService, IAuditService auditService, IUserService userService,
            IContentPublishingService contentPublishingService,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IScopeProvider scopeProvider, ILogger logger, ProfilingLogger proflog)
        {
            _runtime = runtime;
            _contentService = contentService;
            _contentPublishingService = contentPublishingService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _proflog = proflog;

            _healthChecks = healthChecks;
            _notifications = notifications;

            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", logger);
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", logger);
            _tasksRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledTasks", logger);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", logger);
            _healthCheckRunner = new BackgroundTaskRunner<IBackgroundTask>("HealthCheckNotifier", logger);

            // we will start the whole process when a successful request is made
            UmbracoModule.RouteAttempt += RegisterBackgroundTasksOnce;
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
                var settings = UmbracoConfig.For.UmbracoSettings();

                var tasks = new List<IBackgroundTask>();

                tasks.Add(RegisterKeepAlive());
                tasks.Add(RegisterScheduledPublishing());
                tasks.Add(RegisterTaskRunner(settings));
                tasks.Add(RegisterLogScrubber(settings));

                var healthCheckConfig = UmbracoConfig.For.HealthCheck();
                if (healthCheckConfig.NotificationSettings.Enabled)
                    tasks.Add(RegisterHealthCheckNotifier(healthCheckConfig, _healthChecks, _notifications, _logger, _proflog));

                return tasks.ToArray();
            });
        }

        private IBackgroundTask RegisterKeepAlive()
        {
            // ping/keepalive
            // on all servers
            var task = new KeepAlive(_keepAliveRunner, 60000, 300000, _runtime, _logger, _proflog);
            _keepAliveRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterScheduledPublishing()
        {
            // scheduled publishing/unpublishing
            // install on all, will only run on non-replica servers
            var task = new ScheduledPublishing(_publishingRunner, 60000, 60000, _runtime, _contentService, _contentPublishingService, _logger);
            _publishingRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterTaskRunner(IUmbracoSettingsSection settings)
        {
            var task = new ScheduledTasks(_tasksRunner, 60000, 60000, _runtime, settings, _logger, _proflog);
            _tasksRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterHealthCheckNotifier(IHealthChecks healthCheckConfig,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            ILogger logger, ProfilingLogger proflog)
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
            var task = new HealthCheckNotifier(_healthCheckRunner, delayInMilliseconds, periodInMilliseconds, healthChecks, notifications, _runtime, logger, proflog);
            return task;
        }

        private IBackgroundTask RegisterLogScrubber(IUmbracoSettingsSection settings)
        {
            // log scrubbing
            // install on all, will only run on non-replica servers
            var task = new LogScrubber(_scrubberRunner, 60000, LogScrubber.GetLogScrubbingInterval(settings, _logger), _runtime, _auditService, settings, _scopeProvider, _logger, _proflog);
            _scrubberRunner.TryAdd(task);
            return task;
        }
    }
}
