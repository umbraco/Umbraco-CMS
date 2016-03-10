using System.Collections.Generic;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
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

                var tasks = new List<IBackgroundTask>
                {
                    new KeepAlive(_keepAliveRunner, 60000, 300000, e.UmbracoContext.Application),
                    new ScheduledPublishing(_publishingRunner, 60000, 60000, e.UmbracoContext.Application, settings),
                    new ScheduledTasks(_tasksRunner, 60000, 60000, e.UmbracoContext.Application, settings),
                    new LogScrubber(_scrubberRunner, 60000, LogScrubber.GetLogScrubbingInterval(settings), e.UmbracoContext.Application, settings)
                };

                // ping/keepalive
                // on all servers
                _keepAliveRunner.TryAdd(tasks[0]);

                // scheduled publishing/unpublishing
                // install on all, will only run on non-slaves servers
                _publishingRunner.TryAdd(tasks[1]);

                _tasksRunner.TryAdd(tasks[2]);

                // log scrubbing
                // install on all, will only run on non-slaves servers
                _scrubberRunner.TryAdd(tasks[3]);

                return tasks.ToArray();
            });
        }
    }
}
