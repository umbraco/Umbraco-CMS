using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using global::umbraco.BusinessLogic;

namespace Umbraco.Web
{
	// note: has to be public to be detected by the resolver
	//   if it's made internal, which would make more sense, then it's not detected
	//   and it needs to be manually registered - which we want to avoid, in order
	//   to be as unobtrusive as possible

	public class LegacyScheduledTasks : IApplicationEventHandler
	{
		Timer pingTimer;
		Timer publishingTimer;
		CacheItemRemovedCallback OnCacheRemove;

		public void OnApplicationInitialized(UmbracoApplication httpApplication, Core.ApplicationContext applicationContext)
		{
			// nothing yet
		}

		public void OnApplicationStarting(UmbracoApplication httpApplication, Core.ApplicationContext applicationContext)
		{
			// time to setup the tasks

			// these are the legacy tasks
			// just copied over here for backward compatibility
			// of course we should have a proper scheduler, see #U4-809

			// ping/keepalive
			pingTimer = new Timer(new TimerCallback(global::umbraco.presentation.keepAliveService.PingUmbraco), httpApplication.Context, 60000, 300000);

			// (un)publishing _and_ also run scheduled tasks (!)
			publishingTimer = new Timer(new TimerCallback(global::umbraco.presentation.publishingService.CheckPublishing), httpApplication.Context, 30000, 60000);

			// log scrubbing
			AddTask(LOG_SCRUBBER_TASK_NAME, GetLogScrubbingInterval());
		}

		public void OnApplicationStarted(UmbracoApplication httpApplication, Core.ApplicationContext applicationContext)
		{
			// nothing
		}

		#region Log Scrubbing

		// this is a raw copy of the legacy code in all its uglyness

		const string LOG_SCRUBBER_TASK_NAME = "ScrubLogs";

		private static int GetLogScrubbingInterval()
		{
			int interval = 24 * 60 * 60; //24 hours
			try
			{
				if (global::umbraco.UmbracoSettings.CleaningMiliseconds > -1)
					interval = global::umbraco.UmbracoSettings.CleaningMiliseconds;
			}
			catch (Exception)
			{
				Log.Add(LogTypes.System, -1, "Unable to locate a log scrubbing interval.  Defaulting to 24 horus");
			}
			return interval;
		}

		private static int GetLogScrubbingMaximumAge()
		{
			int maximumAge = 24 * 60 * 60;
			try
			{
				if (global::umbraco.UmbracoSettings.MaxLogAge > -1)
					maximumAge = global::umbraco.UmbracoSettings.MaxLogAge;
			}
			catch (Exception)
			{
				Log.Add(LogTypes.System, -1, "Unable to locate a log scrubbing maximum age.  Defaulting to 24 horus");
			}
			return maximumAge;

		}

		private void AddTask(string name, int seconds)
		{
			OnCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
			HttpRuntime.Cache.Insert(name, seconds, null,
				DateTime.Now.AddSeconds(seconds), System.Web.Caching.Cache.NoSlidingExpiration,
				CacheItemPriority.NotRemovable, OnCacheRemove);
		}

		public void CacheItemRemoved(string k, object v, CacheItemRemovedReason r)
		{
			if (k.Equals(LOG_SCRUBBER_TASK_NAME))
			{
				ScrubLogs();
			}
			AddTask(k, Convert.ToInt32(v));
		}

		private static void ScrubLogs()
		{
			Log.CleanLogs(GetLogScrubbingMaximumAge());
		}

		#endregion
	}
}
