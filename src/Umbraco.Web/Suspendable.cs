using System;
using Examine;
using Examine.Providers;
using Umbraco.Core.Composing;
using Umbraco.Web.Cache;

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

                Current.ProfilingLogger.Logger.Info(typeof (PageCacheRefresher), $"Resume document cache (reload:{(_tried ? "true" : "false")}).");

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

            public static void SuspendIndexers()
            {
                Current.ProfilingLogger.Logger.Info(typeof (ExamineEvents), "Suspend indexers.");
                _suspended = true;
            }

            public static void ResumeIndexers()
            {
                _suspended = false;

                Current.ProfilingLogger.Logger.Info(typeof (ExamineEvents), $"Resume indexers (rebuild:{(_tried ? "true" : "false")}).");

                if (_tried == false) return;
                _tried = false;

                // fixme - could we fork this on a background thread?
                foreach (BaseIndexProvider indexer in ExamineManager.Instance.IndexProviderCollection)
                {
                    indexer.RebuildIndex();
                }
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
