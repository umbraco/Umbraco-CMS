using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Web;
using System.Xml;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for publishingService.
	/// </summary>
	public class publishingService
	{
		private static Hashtable scheduledTaskTimes = new Hashtable();
		private static bool isPublishingRunning = false;
		
		public static void CheckPublishing(object sender)
		{
			if(isPublishingRunning)
				return;
			isPublishingRunning = true;
			try
			{
				// DO not run publishing if content is re-loading
				if(!content.Instance.isInitializing)
				{
                    var docs = Document.GetDocumentsForRelease();
					foreach(Document d in docs)
					{
						try
						{
							d.HttpContext = (HttpContext)sender;

                            d.ReleaseDate = DateTime.MinValue; //new DateTime(1, 1, 1); // Causes release date to be null

							d.Publish(d.User);
							library.PublishSingleNode(d.Id);
							
						}
						catch(Exception ee)
						{
							Log.Add(
								LogTypes.Error,
								BusinessLogic.User.GetUser(0),
								d.Id,
								string.Format("Error publishing node: {0}", ee));
						}
					}
					foreach(Document d in Document.GetDocumentsForExpiration())
					{
						d.HttpContext = (HttpContext)sender;
						//d.Published = false;
						library.UnPublishSingleNode(d.Id);
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
								if (!scheduledTaskTimes.ContainsKey(task.Attributes.GetNamedItem("alias").Value))
								{
									runTask = true;
									scheduledTaskTimes.Add(task.Attributes.GetNamedItem("alias").Value, DateTime.Now);
								}
									// Add 1 second to timespan to compensate for differencies in timer
								else if (
									new TimeSpan(DateTime.Now.Ticks -
									             ((DateTime) scheduledTaskTimes[task.Attributes.GetNamedItem("alias").Value]).Ticks).TotalSeconds +
									1 >=
									int.Parse(task.Attributes.GetNamedItem("interval").Value))
								{
									runTask = true;
									scheduledTaskTimes[task.Attributes.GetNamedItem("alias").Value] = DateTime.Now;
								}

								if (runTask)
								{
									bool taskResult = getTaskByHttp(task.Attributes.GetNamedItem("url").Value);
									if (bool.Parse(task.Attributes.GetNamedItem("log").Value))
										Log.Add(LogTypes.ScheduledTask, User.GetUser(0), -1,
										        string.Format("{0} has been called with response: {1}",
										                      task.Attributes.GetNamedItem("alias").Value, taskResult));
								}
							}
						}
					}
				}
				catch(Exception ee)
				{
					Log.Add(LogTypes.Error, User.GetUser(0), -1,
						string.Format("Error executing scheduled task: {0}", ee));
				}
			}
			catch(Exception x)
			{
				Debug.WriteLine(x);
			}
			finally
			{
				isPublishingRunning = false;
			}
		}

		private static bool getTaskByHttp(string url)
		{
			HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
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