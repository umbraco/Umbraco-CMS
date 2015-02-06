using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Publishing;

namespace umbraco.presentation
{
    [Obsolete("This is no longer used and will be removed in future versions")]
	public class publishingService
	{
		private static readonly Hashtable ScheduledTaskTimes = new Hashtable();
		private static bool _isPublishingRunning = false;
		
        //NOTE: sender will be the umbraco ApplicationContext
		public static void CheckPublishing(object sender)
		{
			if(_isPublishingRunning)
				return;
			_isPublishingRunning = true;
			try
			{
                //run the scheduled publishing - we need to determine if this server 

                var publisher = new ScheduledPublisher(ApplicationContext.Current.Services.ContentService);
                publisher.CheckPendingAndProcess();

				// run scheduled url tasks
				try
				{
                    foreach (var t in UmbracoConfig.For.UmbracoSettings().ScheduledTasks.Tasks)
                    {
                        bool runTask = false;
                        if (!ScheduledTaskTimes.ContainsKey(t.Alias))
                        {
                            runTask = true;
                            ScheduledTaskTimes.Add(t.Alias, DateTime.Now);
                        }
                        // Add 1 second to timespan to compensate for differencies in timer
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
                                LogHelper.Info<publishingService>(string.Format("{0} has been called with response: {1}", t.Alias, taskResult));
                        }
                    }
				}
				catch(Exception ee)
				{
                    LogHelper.Error<publishingService>("Error executing scheduled task", ee);
				}
			}
			catch(Exception x)
			{
                LogHelper.Error<publishingService>("Error executing scheduled publishing", x);
			}
			finally
			{
				_isPublishingRunning = false;
			}
		}

		private static bool GetTaskByHttp(string url)
		{
			var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                using (var myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse())
                {
                    return myHttpWebResponse.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<publishingService>("Error sending url request for scheduled task", ex);
            }

		    return false;
		}
	}
}