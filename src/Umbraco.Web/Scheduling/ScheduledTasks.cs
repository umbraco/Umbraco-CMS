using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Sync;
using Umbraco.Core;

namespace Umbraco.Web.Scheduling
{
    //TODO: No scheduled task (i.e. URL) would be secured, so if people are actually using these each task
    // would need to be a publicly available task (URL) which isn't really very good :(
    // We should really be using the AdminTokenAuthorizeAttribute for this stuff

    internal class ScheduledTasks
    {
        private static readonly Hashtable ScheduledTaskTimes = new Hashtable();
        private static bool _isPublishingRunning = false;

        public void Start(ApplicationContext appContext)
        {
            using (DisposableTimer.DebugDuration<ScheduledTasks>(() => "Scheduled tasks executing", () => "Scheduled tasks complete"))
            {
                if (_isPublishingRunning) return;

                _isPublishingRunning = true;
            
                try
                {
                    ProcessTasks();
                }
                catch (Exception ee)
                {
                    LogHelper.Error<ScheduledTasks>("Error executing scheduled task", ee);
                }
                finally
                {
                    _isPublishingRunning = false;
                }
            }
            
        }

        private static void ProcessTasks()
        {


            var scheduledTasks = UmbracoConfig.For.UmbracoSettings().ScheduledTasks.Tasks;
            foreach (var t in scheduledTasks)
            {
                var runTask = false;
                if (!ScheduledTaskTimes.ContainsKey(t.Alias))
                {
                    runTask = true;
                    ScheduledTaskTimes.Add(t.Alias, DateTime.Now);
                }
                /// Add 1 second to timespan to compensate for differencies in timer
                else if (
                    new TimeSpan(
                        DateTime.Now.Ticks - ((DateTime)ScheduledTaskTimes[t.Alias]).Ticks).TotalSeconds + 1 >= t.Interval)
                {
                    runTask = true;
                    ScheduledTaskTimes[t.Alias] = DateTime.Now;
                }

                if (runTask)
                {
                    bool taskResult = GetTaskByHttp(t.Url);
                    if (t.Log)
                        LogHelper.Info<ScheduledTasks>(string.Format("{0} has been called with response: {1}", t.Alias, taskResult));
                }
            }
        }

        private static bool GetTaskByHttp(string url)
        {
            var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            
            try
            {
                using (var response = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ScheduledTasks>("An error occurred calling web task for url: " + url, ex);
            }            

            return false;
        }
    }
}