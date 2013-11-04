using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for publishingService.
	/// </summary>
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
				// DO not run publishing if content is re-loading
				if(!content.Instance.isInitializing)
				{
                   
                    foreach (var d in Document.GetDocumentsForRelease())
					{
						try
						{
                            d.ReleaseDate = DateTime.MinValue; //new DateTime(1, 1, 1); // Causes release date to be null
                            d.SaveAndPublish(d.User);
						}
						catch(Exception ee)
						{
						    LogHelper.Error<publishingService>(string.Format("Error publishing node {0}", d.Id), ee);
						}
					}
					foreach(Document d in Document.GetDocumentsForExpiration())
					{
                        try
                        {
                            d.ExpireDate = DateTime.MinValue;

                            d.UnPublish();
                        }
                        catch (Exception ee)
                        {
                            LogHelper.Error<publishingService>(string.Format("Error unpublishing node {0}", d.Id), ee);
                        }
                       
					}
				}

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