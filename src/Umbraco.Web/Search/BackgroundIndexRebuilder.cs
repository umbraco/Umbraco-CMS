using System;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Web.Scheduling;
using Examine;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Utility to rebuild all indexes on a background thread
    /// </summary>
    public sealed class BackgroundIndexRebuilder
    {
        private static readonly object RebuildLocker = new object();
        private readonly IIndexRebuilder _indexRebuilder;
        private readonly IExamineManager _examineManager;
        private readonly IMainDom _mainDom;
        private readonly IProfilingLogger _logger;
        private static BackgroundTaskRunner<IBackgroundTask> _rebuildOnStartupRunner;

        public BackgroundIndexRebuilder(IMainDom mainDom, IProfilingLogger logger, IIndexRebuilder indexRebuilder, IExamineManager examineManager)
        {
            _mainDom = mainDom;
            _logger = logger;
            _indexRebuilder = indexRebuilder;
            _examineManager = examineManager;
        }

        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        /// <param name="indexRebuilder"></param>
        /// <param name="logger"></param>
        /// <param name="onlyEmptyIndexes"></param>
        /// <param name="waitMilliseconds"></param>
        public void RebuildIndexes(bool onlyEmptyIndexes, int waitMilliseconds = 0)
        {
            // TODO: need a way to disable rebuilding on startup

            lock (RebuildLocker)
            {
                if (_rebuildOnStartupRunner != null && _rebuildOnStartupRunner.IsRunning)
                {
                    _logger.Warn<BackgroundIndexRebuilder>("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
                    return;
                }

                _logger.Info<BackgroundIndexRebuilder>("Starting initialize async background thread.");
                //do the rebuild on a managed background thread
                var task = new RebuildOnStartupTask(_mainDom, _indexRebuilder, _examineManager, _logger, onlyEmptyIndexes, waitMilliseconds);

                _rebuildOnStartupRunner = new BackgroundTaskRunner<IBackgroundTask>(
                    "RebuildIndexesOnStartup",
                    _logger);

                _rebuildOnStartupRunner.TryAdd(task);
            }
        }

        /// <summary>
        /// Background task used to rebuild empty indexes on startup
        /// </summary>
        private class RebuildOnStartupTask : IBackgroundTask
        {
            private readonly IMainDom _mainDom;

            private readonly IIndexRebuilder _indexRebuilder;
            private readonly IExamineManager _examineManager;
            private readonly ILogger _logger;
            private readonly bool _onlyEmptyIndexes;
            private readonly int _waitMilliseconds;

            public RebuildOnStartupTask(
                IMainDom mainDom,
                IIndexRebuilder indexRebuilder,
                IExamineManager examineManager,
                ILogger logger,
                bool onlyEmptyIndexes,
                int waitMilliseconds = 0)
            {
                _mainDom = mainDom;
                _indexRebuilder = indexRebuilder ?? throw new ArgumentNullException(nameof(indexRebuilder));
                _examineManager = examineManager;
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
                    _logger.Error<RebuildOnStartupTask>(ex, "Failed to rebuild empty indexes.");
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
                if (!_mainDom.IsMainDom) return;

                if (_waitMilliseconds > 0)
                    Thread.Sleep(_waitMilliseconds);

                _examineManager.ConfigureIndexes(_mainDom, _logger);
                _indexRebuilder.RebuildIndexes(_onlyEmptyIndexes);
            }
        }
    }
}
