using System;
using System.Threading;
using Examine;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using Umbraco.Web.Scheduling;
using System.Threading.Tasks;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Executes after all other examine components have executed
    /// </summary>
    public sealed class ExamineFinalComponent : IComponent
    {
        private static volatile bool _isConfigured = false;
        private static readonly object IsConfiguredLocker = new object();
        private readonly IProfilingLogger _logger;
        private readonly IExamineManager _examineManager;
        private static readonly object RebuildLocker = new object();
        private readonly IndexRebuilder _indexRebuilder;
        private static BackgroundTaskRunner<IBackgroundTask> _rebuildOnStartupRunner;

        public void Initialize()
        {
            if (!ExamineComponent.ExamineEnabled) return;

            EnsureUnlocked(_logger, _examineManager);

            // TODO: Instead of waiting 5000 ms, we could add an event handler on to fulfilling the first request, then start?
            ExamineComponent.RebuildIndexes(_indexRebuilder, _logger, true, 5000);
        }

        public void Terminate()
        {
        }

        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        /// <param name="indexRebuilder"></param>
        /// <param name="logger"></param>
        /// <param name="onlyEmptyIndexes"></param>
        /// <param name="waitMilliseconds"></param>
        internal static void RebuildIndexes(IndexRebuilder indexRebuilder, ILogger logger, bool onlyEmptyIndexes, int waitMilliseconds = 0)
        {
            // TODO: need a way to disable rebuilding on startup

            lock (RebuildLocker)
            {
                if (_rebuildOnStartupRunner != null && _rebuildOnStartupRunner.IsRunning)
                {
                    logger.Warn<ExamineComponent>("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
                    return;
                }

                logger.Info<ExamineComponent>("Starting initialize async background thread.");
                //do the rebuild on a managed background thread
                var task = new RebuildOnStartupTask(indexRebuilder, logger, onlyEmptyIndexes, waitMilliseconds);

                _rebuildOnStartupRunner = new BackgroundTaskRunner<IBackgroundTask>(
                    "RebuildIndexesOnStartup",
                    logger);

                _rebuildOnStartupRunner.TryAdd(task);
            }
        }

        /// <summary>
        /// Must be called to each index is unlocked before any indexing occurs
        /// </summary>
        /// <remarks>
        /// Indexing rebuilding can occur on a normal boot if the indexes are empty or on a cold boot by the database server messenger. Before
        /// either of these happens, we need to configure the indexes.
        /// </remarks>
        internal static void EnsureUnlocked(ILogger logger, IExamineManager examineManager)
        {
            if (!ExamineComponent.ExamineEnabled) return;
            if (_isConfigured) return;

            lock (IsConfiguredLocker)
            {
                //double check
                if (_isConfigured) return;

                _isConfigured = true;
                examineManager.UnlockLuceneIndexes(logger);
            }
        }

        /// <summary>
        /// Background task used to rebuild empty indexes on startup
        /// </summary>
        private class RebuildOnStartupTask : IBackgroundTask
        {
            private readonly IndexRebuilder _indexRebuilder;
            private readonly ILogger _logger;
            private readonly bool _onlyEmptyIndexes;
            private readonly int _waitMilliseconds;

            public RebuildOnStartupTask(IndexRebuilder indexRebuilder, ILogger logger, bool onlyEmptyIndexes, int waitMilliseconds = 0)
            {
                _indexRebuilder = indexRebuilder ?? throw new ArgumentNullException(nameof(indexRebuilder));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _onlyEmptyIndexes = onlyEmptyIndexes;
                _waitMilliseconds = waitMilliseconds;
            }

            public bool IsAsync => false;

            public void Dispose()
            {
            }

            public void Run()
            {
                try
                {
                    // rebuilds indexes
                    RebuildIndexes();
                }
                catch (Exception ex)
                {
                    _logger.Error<ExamineComponent>(ex, "Failed to rebuild empty indexes.");
                }
            }

            public Task RunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Used to rebuild indexes on startup or cold boot
            /// </summary>
            private void RebuildIndexes()
            {
                //do not attempt to do this if this has been disabled since we are not the main dom.
                //this can be called during a cold boot
                if (!ExamineComponent.ExamineEnabled) return;

                if (_waitMilliseconds > 0)
                    Thread.Sleep(_waitMilliseconds);

                EnsureUnlocked(_logger, _indexRebuilder.ExamineManager);
                _indexRebuilder.RebuildIndexes(_onlyEmptyIndexes);
            }
        }
    }
}
