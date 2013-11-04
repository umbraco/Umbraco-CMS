using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using global::umbraco.BusinessLogic;

namespace Umbraco.Web
{
	
	internal sealed class LegacyScheduledTasks : ApplicationEventHandler
	{
		Timer _pingTimer;
		Timer _publishingTimer;
		CacheItemRemovedCallback _onCacheRemove;

        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, Core.ApplicationContext applicationContext)
        {
            if (umbracoApplication.Context == null)
                return;

			// time to setup the tasks

			// these are the legacy tasks
			// just copied over here for backward compatibility
			// of course we should have a proper scheduler, see #U4-809

			// ping/keepalive
            _pingTimer = new Timer(new TimerCallback(global::umbraco.presentation.keepAliveService.PingUmbraco), applicationContext, 60000, 300000);

			// (un)publishing _and_ also run scheduled tasks (!)
            _publishingTimer = new Timer(new TimerCallback(global::umbraco.presentation.publishingService.CheckPublishing), applicationContext, 30000, 60000);

			// log scrubbing
			AddTask(LOG_SCRUBBER_TASK_NAME, GetLogScrubbingInterval());
		}

		#region Log Scrubbing

		// this is a raw copy of the legacy code in all its uglyness

		const string LOG_SCRUBBER_TASK_NAME = "ScrubLogs";

		private static int GetLogScrubbingInterval()
		{
			int interval = 24 * 60 * 60; //24 hours
			try
			{
				if (UmbracoConfig.For.UmbracoSettings().Logging.CleaningMiliseconds > -1)
					interval = UmbracoConfig.For.UmbracoSettings().Logging.CleaningMiliseconds;
			}
			catch (Exception e)
			{
				LogHelper.Error<LegacyScheduledTasks>("Unable to locate a log scrubbing interval.  Defaulting to 24 horus", e);
			}
			return interval;
		}

		private static int GetLogScrubbingMaximumAge()
		{
			int maximumAge = 24 * 60 * 60;
			try
			{
				if (UmbracoConfig.For.UmbracoSettings().Logging.MaxLogAge > -1)
					maximumAge = UmbracoConfig.For.UmbracoSettings().Logging.MaxLogAge;
			}
			catch (Exception e)
			{
				LogHelper.Error<LegacyScheduledTasks>("Unable to locate a log scrubbing maximum age.  Defaulting to 24 horus", e);
			}
			return maximumAge;

		}

		private void AddTask(string name, int seconds)
		{
			_onCacheRemove = new CacheItemRemovedCallback(CacheItemRemoved);
			HttpRuntime.Cache.Insert(name, seconds, null,
				DateTime.Now.AddSeconds(seconds), System.Web.Caching.Cache.NoSlidingExpiration,
				CacheItemPriority.NotRemovable, _onCacheRemove);
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
