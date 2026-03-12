using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Infrastructure;

/// <summary>
/// Defines an interface for managing the suspension state of implementing classes.
/// </summary>
public static class Suspendable
{
    /// <summary>
    /// Provides functionality to refresh the page cache in a manner that can be suspended and resumed.
    /// This is typically used to temporarily halt cache updates during bulk operations or maintenance.
    /// </summary>
    public static class PageCacheRefresher
    {
        private static bool _tried;
        private static bool _suspended;

        /// <summary>
        /// Gets a value indicating whether the document cache can currently be refreshed from the database.
        /// Returns <c>true</c> if the cache is not suspended; otherwise, returns <c>false</c> and records that a refresh was attempted while suspended.
        /// </summary>
        public static bool CanRefreshDocumentCacheFromDatabase
        {
            get
            {
                // trying a full refresh
                if (_suspended == false)
                {
                    return true;
                }

                _tried = true; // remember we tried
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document cache can be updated.
        /// Returns <c>true</c> if the cache is not suspended, or if a full update has not yet been attempted.
        /// </summary>
        /// <remarks>
        /// trying a partial update
        /// ok if not suspended, or if we haven't done a full already
        /// </remarks>
        public static bool CanUpdateDocumentCache => _suspended == false || _tried == false;

        /// <summary>
        /// Suspends updates to the document cache, preventing cache refresh operations until resumed.
        /// This is typically used during operations where cache consistency must be maintained.
        /// </summary>
        public static void SuspendDocumentCache()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend document cache.");
            _suspended = true;
        }

        /// <summary>
        /// Resumes the document cache if it was previously suspended, and refreshes all cached documents using the provided cache refresher collection.
        /// If the cache was not suspended, no action is taken.
        /// </summary>
        /// <param name="cacheRefresherCollection">The collection of cache refreshers used to refresh the document cache.</param>
        public static void ResumeDocumentCache(CacheRefresherCollection cacheRefresherCollection)
        {
            _suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume document cache (reload:{Tried}).", _tried);

            if (_tried == false)
            {
                return;
            }

            _tried = false;

            ICacheRefresher? pageRefresher = cacheRefresherCollection[ContentCacheRefresher.UniqueId];
            pageRefresher?.RefreshAll();
        }
    }

    /// <summary>
    /// Represents events related to the Examine indexing process that can be suspended.
    /// </summary>
    /// <remarks>
    /// This is really needed at all since the only place this is used is in ExamineComponent and that already maintains a flag of whether it suspsended or not
    /// AHH... but Deploy probably uses this?
    /// </remarks>
    public static class ExamineEvents
    {
        private static bool _tried;
        private static bool _suspended;

        /// <summary>
        /// Gets a value indicating whether indexing is currently allowed.
        /// </summary>
        public static bool CanIndex
        {
            get
            {
                if (_suspended == false)
                {
                    return true;
                }

                _tried = true; // remember we tried
                return false;
            }
        }

        /// <summary>
        /// Suspends the indexers by setting the internal suspended flag and logs the suspension action.
        /// </summary>
        /// <param name="logger">The logger used to record information about the suspension of the indexers.</param>
        public static void SuspendIndexers(ILogger logger)
        {
            logger.LogInformation("Suspend indexers.");
            _suspended = true;
        }

        /// <summary>
        /// Resumes the indexers after they have been suspended. If a rebuild was previously attempted while suspended, this method triggers a rebuild of the indexes using the provided <paramref name="backgroundIndexRebuilder"/>.
        /// </summary>
        /// <param name="backgroundIndexRebuilder">The <see cref="IIndexRebuilder"/> instance used to rebuild the indexes if required.</param>
        public static void ResumeIndexers(IIndexRebuilder backgroundIndexRebuilder)
        {
            _suspended = false;

            StaticApplicationLogging.Logger.LogInformation("Resume indexers (rebuild:{Tried}).", _tried);

            if (_tried == false)
            {
                return;
            }

            _tried = false;

            backgroundIndexRebuilder.RebuildIndexes(false);
        }
    }

    /// <summary>
    /// Represents scheduled publishing operations that can be suspended and resumed.
    /// </summary>
    public static class ScheduledPublishing
    {
        private static bool _suspended;

        /// <summary>
        /// Gets a value indicating whether scheduled publishing can run.
        /// </summary>
        public static bool CanRun => _suspended == false;

        /// <summary>
        /// Suspends the scheduled publishing process.
        /// </summary>
        public static void Suspend()
        {
            StaticApplicationLogging.Logger.LogInformation("Suspend scheduled publishing.");
            _suspended = true;
        }

        /// <summary>
        /// Resumes scheduled publishing operations by clearing the suspended state, allowing scheduled publishing tasks to proceed.
        /// </summary>
        public static void Resume()
        {
            StaticApplicationLogging.Logger.LogInformation("Resume scheduled publishing.");
            _suspended = false;
        }
    }
}
