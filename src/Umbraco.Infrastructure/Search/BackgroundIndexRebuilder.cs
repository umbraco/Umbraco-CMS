using System;
using System.Threading;
using Umbraco.Core.Logging;
using Umbraco.Examine;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Hosting;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Search
{
    /// <summary>
    /// Utility to rebuild all indexes on a background thread
    /// </summary>
    public class BackgroundIndexRebuilder
    {
        private static readonly object RebuildLocker = new object();
        private readonly IndexRebuilder _indexRebuilder;
        private readonly IMainDom _mainDom;
        // TODO: Remove unused ProfilingLogger?
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<BackgroundIndexRebuilder> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IApplicationShutdownRegistry _hostingEnvironment;
        private static BackgroundTaskRunner<IBackgroundTask> _rebuildOnStartupRunner;

        public BackgroundIndexRebuilder(IMainDom mainDom, IProfilingLogger profilingLogger , ILoggerFactory loggerFactory, IApplicationShutdownRegistry hostingEnvironment, IndexRebuilder indexRebuilder)
        {
            _mainDom = mainDom;
            _profilingLogger = profilingLogger ;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<BackgroundIndexRebuilder>();
            _hostingEnvironment = hostingEnvironment;
            _indexRebuilder = indexRebuilder;
        }

        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        /// <param name="onlyEmptyIndexes"></param>
        /// <param name="waitMilliseconds"></param>
        public virtual void RebuildIndexes(bool onlyEmptyIndexes, int waitMilliseconds = 0)
        {
            // TODO: need a way to disable rebuilding on startup

            lock (RebuildLocker)
            {
                if (_rebuildOnStartupRunner != null && _rebuildOnStartupRunner.IsRunning)
                {
                    _logger.LogWarning("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
                    return;
                }

                _logger.LogInformation("Starting initialize async background thread.");
                //do the rebuild on a managed background thread
                var task = new RebuildOnStartupTask(_mainDom, _indexRebuilder, _loggerFactory.CreateLogger<RebuildOnStartupTask>(), onlyEmptyIndexes, waitMilliseconds);

                _rebuildOnStartupRunner = new BackgroundTaskRunner<IBackgroundTask>(
                    "RebuildIndexesOnStartup",
                    _loggerFactory.CreateLogger<BackgroundTaskRunner<IBackgroundTask>>(), _hostingEnvironment);

                _rebuildOnStartupRunner.TryAdd(task);
            }
        }

        /// <summary>
        /// Background task used to rebuild empty indexes on startup
        /// </summary>
        private class RebuildOnStartupTask : IBackgroundTask
        {
            private readonly IMainDom _mainDom;

            private readonly IndexRebuilder _indexRebuilder;
            private readonly ILogger<RebuildOnStartupTask> _logger;
            private readonly bool _onlyEmptyIndexes;
            private readonly int _waitMilliseconds;

            public RebuildOnStartupTask(IMainDom mainDom,
                IndexRebuilder indexRebuilder, ILogger<RebuildOnStartupTask> logger, bool onlyEmptyIndexes, int waitMilliseconds = 0)
            {
                _mainDom = mainDom;
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
                    _logger.LogError(ex, "Failed to rebuild empty indexes.");
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

                _indexRebuilder.RebuildIndexes(_onlyEmptyIndexes);
            }
        }
    }
}
