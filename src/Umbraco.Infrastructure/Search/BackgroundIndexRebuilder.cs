// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Search
{
    /// <summary>
    /// Utility to rebuild all indexes on a background thread
    /// </summary>
    public class BackgroundIndexRebuilder
    {
        private readonly IndexRebuilder _indexRebuilder;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        private readonly IMainDom _mainDom;
        private readonly ILogger<BackgroundIndexRebuilder> _logger;

        private volatile bool _isRunning = false;
        private static readonly object s_rebuildLocker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundIndexRebuilder"/> class.
        /// </summary>
        public BackgroundIndexRebuilder(
            IMainDom mainDom,
            ILogger<BackgroundIndexRebuilder> logger,
            IndexRebuilder indexRebuilder,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _mainDom = mainDom;
            _logger = logger;
            _indexRebuilder = indexRebuilder;
            _backgroundTaskQueue = backgroundTaskQueue;
        }


        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        public virtual void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null)
        {

            lock (s_rebuildLocker)
            {
                if (_isRunning)
                {
                    _logger.LogWarning("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
                    return;
                }

                _logger.LogInformation("Starting initialize async background thread.");

                _backgroundTaskQueue.QueueBackgroundWorkItem(cancellationToken => RebuildIndexes(onlyEmptyIndexes, delay ?? TimeSpan.Zero, cancellationToken));

            }
        }

        private Task RebuildIndexes(bool onlyEmptyIndexes, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (!_mainDom.IsMainDom)
            {
                return Task.CompletedTask;
            }

            if (delay > TimeSpan.Zero)
            {
                Thread.Sleep(delay);
            }

            _isRunning = true;
            _indexRebuilder.RebuildIndexes(onlyEmptyIndexes);
            _isRunning = false;
            return Task.CompletedTask;
        }
    }
}
