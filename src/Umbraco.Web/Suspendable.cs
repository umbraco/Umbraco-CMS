using System;
using Examine;
using Examine.Providers;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Examine;
using Umbraco.Web.Cache;
using Umbraco.Web.Search;

namespace Umbraco.Web
{
    internal static class Suspendable
    {
        public static class PageCacheRefresher
        {
            private static bool _tried, _suspended;

            public static bool CanRefreshDocumentCacheFromDatabase
            {
                get
                {
                    // trying a full refresh
                    if (_suspended == false) return true;
                    _tried = true; // remember we tried
                    return false;
                }
            }

            // trying a partial update
            // ok if not suspended, or if we haven't done a full already
            public static bool CanUpdateDocumentCache => _suspended == false || _tried == false;

            public static void SuspendDocumentCache()
            {
                Current.ProfilingLogger.Logger.Info(typeof (PageCacheRefresher), "Suspend document cache.");
                _suspended = true;
            }

            public static void ResumeDocumentCache()
            {
                _suspended = false;

                Current.ProfilingLogger.Logger.Info(typeof (PageCacheRefresher), "Resume document cache (reload:{Tried}).", _tried);

                if (_tried == false) return;
                _tried = false;

                var pageRefresher = Current.CacheRefreshers[ContentCacheRefresher.UniqueId];
                pageRefresher.RefreshAll();
            }
        }

        public static class ExamineEvents
        {
            private static bool _tried, _suspended;

            public static bool CanIndex
            {
                get
                {
                    if (_suspended == false) return true;
                    _tried = true; // remember we tried
                    return false;
                }
            }

            public static void SuspendIndexers(ILogger logger)
            {
                logger.Info(typeof (ExamineEvents), "Suspend indexers.");
                _suspended = true;
            }

            public static void ResumeIndexers(IndexRebuilder indexRebuilder, ILogger logger)
            {
                _suspended = false;

                logger.Info(typeof (ExamineEvents), "Resume indexers (rebuild:{Tried}).", _tried);

                if (_tried == false) return;
                _tried = false;

                //TODO: when resuming do we always want a full rebuild of all indexes?
                ExamineComponent.RebuildIndexes(indexRebuilder, logger, false);
            }
        }

        public static class ScheduledPublishing
        {
            private static bool _suspended;

            public static bool CanRun => _suspended == false;

            public static void Suspend()
            {
                Current.ProfilingLogger.Logger.Info(typeof (ScheduledPublishing), "Suspend scheduled publishing.");
                _suspended = true;
            }

            public static void Resume()
            {
                Current.ProfilingLogger.Logger.Info(typeof (ScheduledPublishing), "Resume scheduled publishing.");
                _suspended = false;
            }
        }
    }
}
