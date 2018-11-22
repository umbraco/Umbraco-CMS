using System;
using System.Diagnostics;
using Examine;
using Examine.Providers;
using Umbraco.Core;
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

            public static bool CanUpdateDocumentCache
            {
                get
                {
                    // trying a partial update
                    // ok if not suspended, or if we haven't done a full already
                    return _suspended == false || _tried == false;
                }
            }

            public static void SuspendDocumentCache()
            {
                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (PageCacheRefresher), "Suspend document cache.");
                _suspended = true;
            }

            public static void ResumeDocumentCache()
            {
                _suspended = false;

                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (PageCacheRefresher), string.Format("Resume document cache (reload:{0}).", _tried ? "true" : "false"));

                if (_tried == false) return;
                _tried = false;

                var pageRefresher = CacheRefreshersResolver.Current.GetById(new Guid(DistributedCache.PageCacheRefresherId));
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
                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (ExamineEvents), "Suspend indexers.");
                _suspended = true;
            }

            public static void ResumeIndexers()
            {
                _suspended = false;

                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (ExamineEvents), string.Format("Resume indexers (rebuild:{0}).", _tried ? "true" : "false"));

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

            public static bool CanRun
            {
                get { return _suspended == false; }
            }

            public static void Suspend()
            {
                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (ScheduledPublishing), "Suspend scheduled publishing.");
                _suspended = true;
            }

            public static void Resume()
            {
                ApplicationContext.Current.ProfilingLogger.Logger.Info(typeof (ScheduledPublishing), "Resume scheduled publishing.");
                _suspended = false;
            }
        }
    }
}
