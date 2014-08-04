using System;
using System.Web;
using System.Web.Caching;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    //TODO: Refactor this to use a normal scheduling processor!

    internal class LogScrubber
    {
        // this is a raw copy of the legacy code in all its uglyness

        CacheItemRemovedCallback _onCacheRemove;
        const string LogScrubberTaskName = "ScrubLogs";

        public void Start()
        {
            // log scrubbing
            AddTask(LogScrubberTaskName, GetLogScrubbingInterval());
        }

        private static int GetLogScrubbingInterval()
        {
            int interval = 24 * 60 * 60; //24 hours
            try
            {
                if (global::umbraco.UmbracoSettings.CleaningMiliseconds > -1)
                    interval = global::umbraco.UmbracoSettings.CleaningMiliseconds;
            }
            catch (Exception e)
            {
                LogHelper.Error<Scheduler>("Unable to locate a log scrubbing interval.  Defaulting to 24 horus", e);
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
            catch (Exception e)
            {
                LogHelper.Error<Scheduler>("Unable to locate a log scrubbing maximum age.  Defaulting to 24 horus", e);
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
            if (k.Equals(LogScrubberTaskName))
            {
                ScrubLogs();
            }
            AddTask(k, Convert.ToInt32(v));
        }

        private static void ScrubLogs()
        {
            Log.CleanLogs(GetLogScrubbingMaximumAge());
        }
    }
}