using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Logging;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// All tasks are run in a background task runner which is web aware and will wind down the task correctly instead of killing it completely when
    /// the app domain shuts down.
    /// </remarks>
    internal sealed class Scheduler : ApplicationEventHandler
    {
        private BackgroundTaskRunner<IBackgroundTask> _keepAliveRunner;
        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _tasksRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;
        private BackgroundTaskRunner<IBackgroundTask> _healthCheckRunner;
        private bool _started = false;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (umbracoApplication.Context == null)
                return;

            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", applicationContext.ProfilingLogger.Logger);            
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", applicationContext.ProfilingLogger.Logger);
            _tasksRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledTasks", applicationContext.ProfilingLogger.Logger);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", applicationContext.ProfilingLogger.Logger);
            _healthCheckRunner = new BackgroundTaskRunner<IBackgroundTask>("HealthCheckNotifier", applicationContext.ProfilingLogger.Logger);

            //We will start the whole process when a successful request is made
            UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }

        private void UmbracoModuleRouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                    RegisterBackgroundTasks(e);
                    break;
            }
        }

        private void RegisterBackgroundTasks(UmbracoRequestEventArgs e)
        {
            //remove handler, we're done
            UmbracoModule.RouteAttempt -= UmbracoModuleRouteAttempt;

            LazyInitializer.EnsureInitialized(ref _tasks, ref _started, ref _locker, () =>
            {
                LogHelper.Debug<Scheduler>(() => "Initializing the scheduler");
                var settings = UmbracoConfig.For.UmbracoSettings();

                var healthCheckConfig = UmbracoConfig.For.HealthCheck();

                const int delayMilliseconds = 60000;
                var tasks = new List<IBackgroundTask>
                {
                    new KeepAlive(_keepAliveRunner, delayMilliseconds, 300000, e.UmbracoContext.Application),
                    new ScheduledPublishing(_publishingRunner, delayMilliseconds, 60000, e.UmbracoContext.Application, settings),
                    new ScheduledTasks(_tasksRunner, delayMilliseconds, 60000, e.UmbracoContext.Application, settings),
                    new LogScrubber(_scrubberRunner, delayMilliseconds, LogScrubber.GetLogScrubbingInterval(settings), e.UmbracoContext.Application, settings)
                };

                if (healthCheckConfig.NotificationSettings.Enabled)
                {
                    // If first run time not set, start with just small delay after application start
                    int delayInMilliseconds;
                    if (string.IsNullOrEmpty(healthCheckConfig.NotificationSettings.FirstRunTime))
                    {
                        delayInMilliseconds = delayMilliseconds;
                    }
                    else
                    {
                        // Otherwise start at scheduled time
                        delayInMilliseconds = DateTime.Now.PeriodicMinutesFrom(healthCheckConfig.NotificationSettings.FirstRunTime) * 60 * 1000;
                        if (delayInMilliseconds < delayMilliseconds)
                        {
                            delayInMilliseconds = delayMilliseconds;
                        }
                    }

                    var periodInMilliseconds = healthCheckConfig.NotificationSettings.PeriodInHours * 60 * 60 * 1000;
                    tasks.Add(new HealthCheckNotifier(_healthCheckRunner, delayInMilliseconds, periodInMilliseconds, e.UmbracoContext.Application));
                }

                // ping/keepalive
                // on all servers
                _keepAliveRunner.TryAdd(tasks[0]);

                // scheduled publishing/unpublishing
                // install on all, will only run on non-replica servers
                _publishingRunner.TryAdd(tasks[1]);

                _tasksRunner.TryAdd(tasks[2]);

                // log scrubbing
                // install on all, will only run on non-replica servers
                _scrubberRunner.TryAdd(tasks[3]);

                if (healthCheckConfig.NotificationSettings.Enabled)
                {
                    _healthCheckRunner.TryAdd(tasks[4]);
                }

                OnInitializing(tasks);

                return tasks.ToArray();
            });
        }

        public static event EventHandler<List<IBackgroundTask>> Initializing;

        private static void OnInitializing(List<IBackgroundTask> e)
        {
            var handler = Initializing;
            if (handler != null) handler(null, e);
        }
    }
}
