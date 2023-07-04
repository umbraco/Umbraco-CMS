// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Search.Indexing;

namespace Umbraco.Search;

public class IndexRebuilder : IIndexRebuilder
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly ILogger<IndexRebuilder> _logger;
    private readonly IMainDom _mainDom;
    private readonly IEnumerable<IIndexPopulator> _populators;
    private readonly object _rebuildLocker = new();
    private readonly IRuntimeState _runtimeState;
    private readonly ISearchProvider _provider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IndexRebuilder" /> class.
    /// </summary>
    public IndexRebuilder(
        IMainDom mainDom,
        IRuntimeState runtimeState,
        ISearchProvider provider,
        ILogger<IndexRebuilder> logger,
        IEnumerable<IIndexPopulator> populators,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _mainDom = mainDom;
        _runtimeState = runtimeState;
        _provider = provider;
        _logger = logger;
        _populators = populators;
        _backgroundTaskQueue = backgroundTaskQueue;
    }

    public bool CanRebuild(string indexName)
    {
        return _populators.Any(x => x.IsRegistered(indexName));
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
            _logger.LogInformation("Starting async background thread for rebuilding index {indexName}.", indexName);

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                cancellationToken => Task.Run(() => RebuildIndexInt(indexName, delay.Value, cancellationToken)));
        }
        else
        {
            RebuildIndexInt(indexName, delay.Value, CancellationToken.None);
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
            _logger.LogDebug($"Queuing background job for {nameof(RebuildIndexes)}.");

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                cancellationToken =>
                {
                    // This is a fire/forget task spawned by the background thread queue (which means we
                    // don't need to worry about ExecutionContext flowing).
                    Task.Run(() => RebuildIndexesINT(delay.Value, cancellationToken));

                    // immediately return so the queue isn't waiting.
                    return Task.CompletedTask;
                });
        }
        else
        {
            RebuildIndexesINT(delay.Value, CancellationToken.None);
        }
    }

    private bool CanRun() => _mainDom.IsMainDom && _runtimeState.Level == RuntimeLevel.Run;

    private void RebuildIndexInt(string indexName, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            Thread.Sleep(delay);
        }

        try
        {
            if (!Monitor.TryEnter(_rebuildLocker))
            {
                _logger.LogWarning(
                    "Call was made to RebuildIndexes but the task runner for rebuilding is already running");
            }
            else
            {

                    _provider.CreateIndex(indexName);

                // run each populator over the indexes
                foreach (IIndexPopulator populator in _populators)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        populator.Populate(indexName);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Index populating failed for populator {Populator}", populator.GetType());
                    }
                }
            }
        }
        finally
        {
            if (Monitor.IsEntered(_rebuildLocker))
            {
                Monitor.Exit(_rebuildLocker);
            }
        }
    }

    private void RebuildIndexesINT(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            Thread.Sleep(delay);
        }

        try
        {
            if (!Monitor.TryEnter(_rebuildLocker))
            {
                _logger.LogWarning(
                    $"Call was made to {nameof(RebuildIndexes)} but the task runner for rebuilding is already running");
            }
            else
            {
                // If an index exists but it has zero docs we'll consider it empty and rebuild
                IEnumerable<string> indexes = _provider.GetUnhealthyIndexes();
                if (!indexes.Any())
                {
                    return;
                }

                foreach (var index in indexes)
                {
                    _provider.CreateIndex(index);
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
                        populator.Populate(indexes.ToArray());
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Index populating failed for populator {Populator}", populator.GetType());
                    }
                }
            }
        }
        finally
        {
            if (Monitor.IsEntered(_rebuildLocker))
            {
                Monitor.Exit(_rebuildLocker);
            }
        }
    }
}
