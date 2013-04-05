using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml;
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
					XmlNode scheduledTasks = UmbracoSettings.ScheduledTasks;
					if(scheduledTasks != null)
					{
						XmlNodeList tasks = scheduledTasks.SelectNodes("./task");
						if(tasks != null)
						{
							foreach (XmlNode task in tasks)
							{
								bool runTask = false;
								if (!ScheduledTaskTimes.ContainsKey(task.Attributes.GetNamedItem("alias").Value))
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
									bool taskResult = getTaskByHttp(task.Attributes.GetNamedItem("url").Value);
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

		private static bool getTaskByHttp(string url)
		{
			var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse myHttpWebResponse = null;
			try
			{
				myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
				if(myHttpWebResponse.StatusCode == HttpStatusCode.OK)
				{
					myHttpWebResponse.Close();
					return true;
				}
				else
				{
					myHttpWebResponse.Close();
					return false;
				}
			}
			catch
			{
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