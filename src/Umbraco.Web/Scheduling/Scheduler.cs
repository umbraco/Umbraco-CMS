using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// 
    /// TODO: Much of this code is legacy and needs to be updated, there are a few new/better ways to do scheduling
    /// in a web project nowadays. 
    /// 
    /// //TODO: We need a much more robust way of handing scheduled tasks and also need to take into account app shutdowns during 
    /// a scheduled tasks operation
    /// http://haacked.com/archive/2011/10/16/the-dangers-of-implementing-recurring-background-tasks-in-asp-net.aspx/
    /// 
    /// </remarks>
    internal sealed class Scheduler : ApplicationEventHandler
    {
        private static Timer _pingTimer;
        private static Timer _schedulingTimer;
        private static LogScrubber _scrubber;

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            if (umbracoApplication.Context == null)
                return;

            LogHelper.Debug<Scheduler>(() => "Initializing the scheduler");

            // time to setup the tasks

            // these are the legacy tasks
            // just copied over here for backward compatibility
            // of course we should have a proper scheduler, see #U4-809

            //NOTE: It is important to note that we need to use the ctor for a timer without the 'state' object specified, this is in order 
            // to ensure that the timer itself is not GC'd since internally .net will pass itself in as the state object and that will keep it alive.
            // There's references to this here: http://stackoverflow.com/questions/4962172/why-does-a-system-timers-timer-survive-gc-but-not-system-threading-timer
            // we also make these timers static to ensure further GC safety.

            // ping/keepalive            
            _pingTimer = new Timer(KeepAlive.Start);
            _pingTimer.Change(60000, 300000);

            // scheduled publishing/unpublishing
            _schedulingTimer = new Timer(PerformScheduling);
            _schedulingTimer.Change(30000, 60000);

            //log scrubbing
            _scrubber = new LogScrubber();
            _scrubber.Start();
        }

        /// <summary>
        /// This performs all of the scheduling on the one timer
        /// </summary>
        /// <param name="sender"></param>
        /// <remarks>
        /// No processing will be done if this server is a slave
        /// </remarks>
        private static void PerformScheduling(object sender)
        {
            using (DisposableTimer.DebugDuration<Scheduler>(() => "Scheduling interval executing", () => "Scheduling interval complete"))
            {
                //get the current server status to see if this server should execute the scheduled publishing
                var serverStatus = ServerEnvironmentHelper.GetStatus();

                switch (serverStatus)
                {
                    case CurrentServerEnvironmentStatus.Single:
                    case CurrentServerEnvironmentStatus.Master:
                    case CurrentServerEnvironmentStatus.Unknown:
                        //if it's a single server install, a master or it cannot be determined 
                        // then we will process the scheduling
                    
                        //do the scheduled publishing
                        var scheduledPublishing = new ScheduledPublishing();
                        scheduledPublishing.Start(ApplicationContext.Current);

                        //do the scheduled tasks
                        var scheduledTasks = new ScheduledTasks();
                        scheduledTasks.Start(ApplicationContext.Current);

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
