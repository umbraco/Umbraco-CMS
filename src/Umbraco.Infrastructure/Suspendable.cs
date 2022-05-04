using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure;

public static class Suspendable
{
    public static class PageCacheRefresher
    {
        private static bool tried;
        private static bool suspended;

        public static bool CanRefreshDocumentCacheFromDatabase
        {
            get
            {
                // trying a full refresh
                if (suspended == false)
                {
                    return true;
                }

                tried = true; // remember we tried
                return false;
            }
        }

        // trying a partial update
        // ok if not suspended, or if we haven't done a full already
        public static bool CanUpdateDocumentCache => suspended == false || tried == false;

        public static void SuspendDocumentCache()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend document cache.");
            suspended = true;
        }

        public static void ResumeDocumentCache(CacheRefresherCollection cacheRefresherCollection)
        {
            suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume document cache (reload:{Tried}).", tried);

            if (tried == false)
            {
                return;
            }

            tried = false;

            ICacheRefresher? pageRefresher = cacheRefresherCollection[ContentCacheRefresher.UniqueId];
            pageRefresher?.RefreshAll();
        }
    }

    // This is really needed at all since the only place this is used is in ExamineComponent and that already maintains a flag of whether it suspsended or not
    // AHH... but Deploy probably uses this?
    public static class ExamineEvents
    {
        private static bool tried;
        private static bool suspended;

        public static bool CanIndex
        {
            get
            {
                if (suspended == false)
                {
                    return true;
                }

                tried = true; // remember we tried
                return false;
            }
        }

        public static void SuspendIndexers(ILogger logger)
        {
            logger.LogInformation("Suspend indexers.");
            suspended = true;
        }

        public static void ResumeIndexers(ExamineIndexRebuilder backgroundIndexRebuilder)
        {
            suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume indexers (rebuild:{Tried}).", tried);

            if (tried == false)
            {
                return;
            }

            tried = false;

            backgroundIndexRebuilder.RebuildIndexes(false);
        }
    }

    public static class ScheduledPublishing
    {
        private static bool suspended;

        public static bool CanRun => suspended == false;

        public static void Suspend()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend scheduled publishing.");
            suspended = true;
        }

        public static void Resume()
        {
            StaticApplicationLogging.Logger.LogInformation("Resume scheduled publishing.");
            suspended = false;
        }
    }
}
