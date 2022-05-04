using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure;

public static class Suspendable
{
    public static class PageCacheRefresher
    {
        private static bool s_tried, s_suspended;

        public static bool CanRefreshDocumentCacheFromDatabase
        {
            get
            {
                // trying a full refresh
                if (s_suspended == false)
                {
                    return true;
                }

                s_tried = true; // remember we tried
                return false;
            }
        }

        // trying a partial update
        // ok if not suspended, or if we haven't done a full already
        public static bool CanUpdateDocumentCache => s_suspended == false || s_tried == false;

        public static void SuspendDocumentCache()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend document cache.");
            s_suspended = true;
        }

        public static void ResumeDocumentCache(CacheRefresherCollection cacheRefresherCollection)
        {
            s_suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume document cache (reload:{Tried}).", s_tried);

            if (s_tried == false)
            {
                return;
            }

            s_tried = false;

            ICacheRefresher? pageRefresher = cacheRefresherCollection[ContentCacheRefresher.UniqueId];
            pageRefresher?.RefreshAll();
        }
    }

    //This is really needed at all since the only place this is used is in ExamineComponent and that already maintains a flag of whether it suspsended or not
    // AHH... but Deploy probably uses this?
    public static class ExamineEvents
    {
        private static bool s_tried, s_suspended;

        public static bool CanIndex
        {
            get
            {
                if (s_suspended == false)
                {
                    return true;
                }

                s_tried = true; // remember we tried
                return false;
            }
        }

        public static void SuspendIndexers(ILogger logger)
        {
            logger.LogInformation("Suspend indexers.");
            s_suspended = true;
        }

        public static void ResumeIndexers(ExamineIndexRebuilder backgroundIndexRebuilder)
        {
            s_suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume indexers (rebuild:{Tried}).", s_tried);

            if (s_tried == false)
            {
                return;
            }

            s_tried = false;

            backgroundIndexRebuilder.RebuildIndexes(false);
        }
    }

    public static class ScheduledPublishing
    {
        private static bool s_suspended;

        public static bool CanRun => s_suspended == false;

        public static void Suspend()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend scheduled publishing.");
            s_suspended = true;
        }

        public static void Resume()
        {
            StaticApplicationLogging.Logger.LogInformation("Resume scheduled publishing.");
            s_suspended = false;
        }
    }
}
