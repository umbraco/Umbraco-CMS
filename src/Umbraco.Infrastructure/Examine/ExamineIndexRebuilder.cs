// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Infrastructure.Examine
{
    public class ExamineIndexRebuilder : IIndexRebuilder
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IMainDom _mainDom;
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger<ExamineIndexRebuilder> _logger;
        private readonly IExamineManager _examineManager;
        private readonly IEnumerable<IIndexPopulator> _populators;
        private readonly object _rebuildLocker = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExamineIndexRebuilder"/> class.
        /// </summary>
        public ExamineIndexRebuilder(
            IMainDom mainDom,
            IRuntimeState runtimeState,
            ILogger<ExamineIndexRebuilder> logger,
            IExamineManager examineManager,
            IEnumerable<IIndexPopulator> populators,
            IBackgroundTaskQueue backgroundTaskQueue)
        {
            _mainDom = mainDom;
            _runtimeState = runtimeState;
            _logger = logger;
            _examineManager = examineManager;
            _populators = populators;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public bool CanRebuild(string indexName)
        {
            if (!_examineManager.TryGetIndex(indexName, out IIndex index))
            {
                throw new InvalidOperationException("No index found by name " + indexName);
            }

            return _populators.Any(x => x.IsRegistered(index));
        }

        public virtual void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true)
        {
            if (delay == null)
            {
                delay = TimeSpan.Zero;
            }

            if (!CanRun())
            {
                return;
            }

            if (useBackgroundThread)
            {
                _logger.LogInformation($"Starting async background thread for rebuilding index {indexName}.");

                _backgroundTaskQueue.QueueBackgroundWorkItem(
                    cancellationToken => Task.Run(() => RebuildIndex(indexName, delay.Value, cancellationToken)));
            }
            else
            {
                RebuildIndex(indexName, delay.Value, CancellationToken.None);
            }
        }

        public virtual void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
        {
            if (delay == null)
            {
                delay = TimeSpan.Zero;
            }

            if (!CanRun())
            {
                return;
            }

            if (useBackgroundThread)
            {
                _logger.LogInformation($"Starting async background thread for {nameof(RebuildIndexes)}.");

                _backgroundTaskQueue.QueueBackgroundWorkItem(
                    cancellationToken => Task.Run(() => RebuildIndexes(onlyEmptyIndexes, delay.Value, cancellationToken)));
            }
            else
            {
                RebuildIndexes(onlyEmptyIndexes, delay.Value, CancellationToken.None);
            }
        }

        private bool CanRun() => _mainDom.IsMainDom && _runtimeState.Level >= RuntimeLevel.Run;

        private void RebuildIndex(string indexName, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (delay > TimeSpan.Zero)
            {
                Thread.Sleep(delay);
            }

            if (!Monitor.TryEnter(_rebuildLocker))
            {
                _logger.LogWarning("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
            }
            else
            {
                if (!_examineManager.TryGetIndex(indexName, out IIndex index))
                {
                    throw new InvalidOperationException($"No index found with name {indexName}");
                }

                index.CreateIndex(); // clear the index
                foreach (IIndexPopulator populator in _populators)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    populator.Populate(index);
                }
            }
        }

        private void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan delay, CancellationToken cancellationToken)
        {
            if (delay > TimeSpan.Zero)
            {
                Thread.Sleep(delay);
            }

            if (!Monitor.TryEnter(_rebuildLocker))
            {
                _logger.LogWarning($"Call was made to {nameof(RebuildIndexes)} but the task runner for rebuilding is already running");
            }
            else
            {
                IIndex[] indexes = (onlyEmptyIndexes
                  ? _examineManager.Indexes.Where(x => !x.IndexExists())
                  : _examineManager.Indexes).ToArray();

                if (indexes.Length == 0)
                {
                    return;
                }

                foreach (IIndex index in indexes)
                {
                    index.CreateIndex(); // clear the index
                }

                // run each populator over the indexes
                foreach (IIndexPopulator populator in _populators)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        populator.Populate(indexes);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Index populating failed for populator {Populator}", populator.GetType());
                    }
                }
            }
        }
    }
}
