using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml;
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
					XmlNode scheduledTasks = UmbracoSettings.ScheduledTasks;
					if(scheduledTasks != null)
					{
						XmlNodeList tasks = scheduledTasks.SelectNodes("./task");
						if(tasks != null)
						{
							foreach (XmlNode task in tasks)
							{
								bool runTask = false;
								if (ScheduledTaskTimes.ContainsKey(task.Attributes.GetNamedItem("alias").Value) == false)
								{
									runTask = true;
									ScheduledTaskTimes.Add(task.Attributes.GetNamedItem("alias").Value, DateTime.Now);
								}
									// Add 1 second to timespan to compensate for differencies in timer
								else if (
									new TimeSpan(DateTime.Now.Ticks -
									             ((DateTime) ScheduledTaskTimes[task.Attributes.GetNamedItem("alias").Value]).Ticks).TotalSeconds +
									1 >=
									int.Parse(task.Attributes.GetNamedItem("interval").Value))
								{
									runTask = true;
									ScheduledTaskTimes[task.Attributes.GetNamedItem("alias").Value] = DateTime.Now;
								}

								if (runTask)
								{
									bool taskResult = GetTaskByHttp(task.Attributes.GetNamedItem("url").Value);
									if (bool.Parse(task.Attributes.GetNamedItem("log").Value))
                                        LogHelper.Info<publishingService>(string.Format("{0} has been called with response: {1}", task.Attributes.GetNamedItem("alias").Value, taskResult));
								}
							}
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
				Debug.WriteLine(x);
			}
			finally
			{
				_isPublishingRunning = false;
			}
		}

		private static bool GetTaskByHttp(string url)
		{
			var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse myHttpWebResponse = null;
			try
			{
			    using (myHttpWebResponse = (HttpWebResponse) myHttpWebRequest.GetResponse())
			    {
                    return myHttpWebResponse.StatusCode == HttpStatusCode.OK;
			    }
			}
			catch (Exception ex)
			{
                LogHelper.Error<publishingService>("An error occurred calling web task for url: " + url, ex);
			}
			finally
			{
				// Release the HttpWebResponse Resource.
				if(myHttpWebResponse != null)
					myHttpWebResponse.Close();
			}

			return false;
		}
	}
}