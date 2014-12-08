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
        private static Timer _schedulingTimer;
        private static BackgroundTaskRunner<ScheduledPublishing> _publishingRunner;
        private static BackgroundTaskRunner<ScheduledTasks> _tasksRunner;
        private static BackgroundTaskRunner<LogScrubber> _scrubberRunner;
        private static Timer _logScrubberTimer;
        private static volatile bool _started = false;
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

                                // time to setup the tasks

                                //We have 3 background runners that are web aware, if the app domain dies, these tasks will wind down correctly
                                _publishingRunner = new BackgroundTaskRunner<ScheduledPublishing>();
                                _tasksRunner = new BackgroundTaskRunner<ScheduledTasks>();
                                _scrubberRunner = new BackgroundTaskRunner<LogScrubber>();

                                //NOTE: It is important to note that we need to use the ctor for a timer without the 'state' object specified, this is in order 
                                // to ensure that the timer itself is not GC'd since internally .net will pass itself in as the state object and that will keep it alive.
                                // There's references to this here: http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer
                                // we also make these timers static to ensure further GC safety.

                                // ping/keepalive - NOTE: we don't use a background runner for this because it does not need to be web aware, if the app domain dies, no problem
                                _pingTimer = new Timer(state => KeepAlive.Start(applicationContext, UmbracoConfig.For.UmbracoSettings()));
                                _pingTimer.Change(60000, 300000);

                                // scheduled publishing/unpublishing
                                _schedulingTimer = new Timer(state => PerformScheduling(applicationContext, UmbracoConfig.For.UmbracoSettings()));
                                _schedulingTimer.Change(60000, 60000);

                                //log scrubbing
                                _logScrubberTimer = new Timer(state => PerformLogScrub(applicationContext, UmbracoConfig.For.UmbracoSettings()));
                                _logScrubberTimer.Change(60000, GetLogScrubbingInterval(UmbracoConfig.For.UmbracoSettings()));
                            }
                        }
                    }
                };
            };
        }


        private int GetLogScrubbingInterval(IUmbracoSettingsSection settings)
        {
            int interval = 24 * 60 * 60; //24 hours
            try
            {
                if (settings.Logging.CleaningMiliseconds > -1)
                    interval = settings.Logging.CleaningMiliseconds;
            }
            catch (Exception e)
            {
                LogHelper.Error<Scheduler>("Unable to locate a log scrubbing interval.  Defaulting to 24 horus", e);
            }
            return interval;
        }

        private static void PerformLogScrub(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            _scrubberRunner.Add(new LogScrubber(appContext, settings));
        }

        /// <summary>
        /// This performs all of the scheduling on the one timer
        /// </summary>
        /// <param name="appContext"></param>
        /// <param name="settings"></param>
        /// <remarks>
        /// No processing will be done if this server is a slave
        /// </remarks>
        private static void PerformScheduling(ApplicationContext appContext, IUmbracoSettingsSection settings)
        {
            using (DisposableTimer.DebugDuration<Scheduler>(() => "Scheduling interval executing", () => "Scheduling interval complete"))
            {
                //get the current server status to see if this server should execute the scheduled publishing
                var serverStatus = ServerEnvironmentHelper.GetStatus(settings);

                switch (serverStatus)
                {
                    case CurrentServerEnvironmentStatus.Single:
                    case CurrentServerEnvironmentStatus.Master:
                    case CurrentServerEnvironmentStatus.Unknown:
                        //if it's a single server install, a master or it cannot be determined 
                        // then we will process the scheduling
                    
                        //do the scheduled publishing
                        _publishingRunner.Add(new ScheduledPublishing(appContext, settings));
     
                        //do the scheduled tasks
                        _tasksRunner.Add(new ScheduledTasks(appContext, settings));
                        
                        break;
                    case CurrentServerEnvironmentStatus.Slave:                
                        //do not process
                        
                        LogHelper.Debug<Scheduler>(
                            () => string.Format("Current server ({0}) detected as a slave, no scheduled processes will execute on this server", NetworkHelper.MachineName));

                        break;
                }            
            }            
        }
        
    }
}
