using System;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

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
        private static Timer _pingTimer;
        private static BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private static BackgroundTaskRunner<IBackgroundTask> _tasksRunner;
        private static BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;
        private static volatile bool _started;
        private static readonly object Locker = new object();

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (umbracoApplication.Context == null)
                return;

            //subscribe to app init so we can subsribe to the application events
            UmbracoApplicationBase.ApplicationInit += (sender, args) =>
            {
                var app = (HttpApplication)sender;

                //subscribe to the end of a successful request (a handler actually executed)
                app.PostRequestHandlerExecute += (o, eventArgs) =>
                {
                    if (_started == false)
                    {
                        lock (Locker)
                        {
                            if (_started == false)
                            {
                                _started = true;
                                LogHelper.Debug<Scheduler>(() => "Initializing the scheduler");

                                // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
                                _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>(applicationContext.ProfilingLogger.Logger);
                                _tasksRunner = new BackgroundTaskRunner<IBackgroundTask>(applicationContext.ProfilingLogger.Logger);
                                _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>(applicationContext.ProfilingLogger.Logger);

                                var settings = UmbracoConfig.For.UmbracoSettings();

                                // note
                                // must use the single-parameter constructor on Timer to avoid it from being GC'd
                                // also make the timer static to ensure further GC safety
                                // read http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer

                                // ping/keepalive - no need for a background runner - does not need to be web aware, ok if the app domain dies
                                _pingTimer = new Timer(state => KeepAlive.Start(applicationContext, UmbracoConfig.For.UmbracoSettings()));
                                _pingTimer.Change(60000, 300000);

                                // scheduled publishing/unpublishing
                                // install on all, will only run on non-slaves servers
                                // both are delayed recurring tasks
                                _publishingRunner.Add(new ScheduledPublishing(_publishingRunner, 60000, 60000, applicationContext, settings));
                                _tasksRunner.Add(new ScheduledTasks(_tasksRunner, 60000, 60000, applicationContext, settings));

                                // log scrubbing
                                // install & run on all servers
                                // LogScrubber is a delayed recurring task
                                _scrubberRunner.Add(new LogScrubber(_scrubberRunner, 60000, LogScrubber.GetLogScrubbingInterval(settings), applicationContext, settings));
                            }
                        }
                    }
                };
            };
        }
    }
}
