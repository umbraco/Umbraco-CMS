// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal class ExamineIndexRebuilder : IIndexRebuilder
{
    private const string RebuildAllKey = "__rebuild_all__";

    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly ConcurrentDictionary<string, byte> _rebuilding = new();
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ExamineIndexRebuilder> _logger;
    private readonly IMainDom _mainDom;
    private readonly IEnumerable<IIndexPopulator> _populators;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExamineIndexRebuilder" /> class.
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

    /// <inheritdoc/>
    public bool CanRebuild(string indexName)
    {
        if (!_examineManager.TryGetIndex(indexName, out IIndex index))
        {
            throw new InvalidOperationException("No index found by name " + indexName);
        }

        return HasRegisteredPopulator(index);
    }

    /// <inheritdoc/>
    [Obsolete("Use RebuildIndexAsync() instead. Scheduled for removal in Umbraco 19.")]
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
                cancellationToken =>
                {
                    // Do not flow AsyncLocal to the child thread
                    using (ExecutionContext.SuppressFlow())
                    {
                        Task.Run(() => RebuildIndexCoreAsync(indexName, delay.Value, cancellationToken));

                        // immediately return so the queue isn't waiting.
                        return Task.CompletedTask;
                    }
                });
        }
        else
        {
            RebuildIndexCoreAsync(indexName, delay.Value, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<IndexRebuildResult>> RebuildIndexAsync(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        if (!CanRun())
        {
            return Attempt.Fail(IndexRebuildResult.NotAllowedToRun);
        }

        if (!_examineManager.TryGetIndex(indexName, out IIndex index))
        {
            return Attempt.Fail(IndexRebuildResult.Unknown);
        }

        if (!HasRegisteredPopulator(index))
        {
            return Attempt.Fail(IndexRebuildResult.Unknown);
        }

        if (useBackgroundThread)
        {
            _logger.LogInformation("Starting async background thread for rebuilding index {indexName}.", indexName);

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                ct => RebuildIndexCoreAsync(indexName, delay ?? TimeSpan.Zero, ct));
        }
        else
        {
            await RebuildIndexCoreAsync(indexName, delay ?? TimeSpan.Zero, CancellationToken.None);
        }

        return Attempt.Succeed(IndexRebuildResult.Success);
    }

    /// <inheritdoc/>
    [Obsolete("Use RebuildIndexesAsync() instead. Scheduled for removal in Umbraco 19.")]
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
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                _logger.LogDebug($"Queuing background job for {nameof(RebuildIndexes)}.");
            }

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                cancellationToken =>
                {
                    // Do not flow AsyncLocal to the child thread
                    using (ExecutionContext.SuppressFlow())
                    {
                        // This is a fire/forget task spawned by the background thread queue (which means we
                        // don't need to worry about ExecutionContext flowing).
                        Task.Run(() => RebuildIndexesCoreAsync(onlyEmptyIndexes, delay.Value, cancellationToken));

                        // immediately return so the queue isn't waiting.
                        return Task.CompletedTask;
                    }
                });
        }
        else
        {
            RebuildIndexesCoreAsync(onlyEmptyIndexes, delay.Value, CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<Attempt<IndexRebuildResult>> RebuildIndexesAsync(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        if (!CanRun())
        {
            return Attempt.Fail(IndexRebuildResult.NotAllowedToRun);
        }

        if (useBackgroundThread)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Queuing background job for {nameof(RebuildIndexes)}.");
            }

            _backgroundTaskQueue.QueueBackgroundWorkItem(
                ct => RebuildIndexesCoreAsync(onlyEmptyIndexes, delay ?? TimeSpan.Zero, ct));
        }
        else
        {
            await RebuildIndexesCoreAsync(onlyEmptyIndexes, delay ?? TimeSpan.Zero, CancellationToken.None);
        }

        return Attempt.Succeed(IndexRebuildResult.Success);
    }

    /// <inheritdoc/>
    public Task<bool> IsRebuildingAsync(string indexName)
        => Task.FromResult(_rebuilding.ContainsKey(indexName) || _rebuilding.ContainsKey(RebuildAllKey));

    private bool CanRun() => _mainDom.IsMainDom && _runtimeState.Level == RuntimeLevel.Run;

    private async Task RebuildIndexCoreAsync(string indexName, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        if (!_rebuilding.TryAdd(indexName, 0))
        {
            _logger.LogWarning("Call was made to RebuildIndex but a rebuild for {IndexName} is already running", indexName);
            return;
        }

        try
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
        finally
        {
            _rebuilding.TryRemove(indexName, out _);
        }
    }

    private async Task RebuildIndexesCoreAsync(bool onlyEmptyIndexes, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        if (!_rebuilding.TryAdd(RebuildAllKey, 0))
        {
            _logger.LogWarning($"Call was made to {nameof(RebuildIndexes)} but the task runner for rebuilding is already running");
            return;
        }

        try
        {
            // If an index exists but it has zero docs we'll consider it empty and rebuild
            IIndex[] indexes = (onlyEmptyIndexes
                ? _examineManager.Indexes.Where(ShouldRebuild)
                : _examineManager.Indexes)
                .Where(HasRegisteredPopulator)
                .ToArray();

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
        finally
        {
            _rebuilding.TryRemove(RebuildAllKey, out _);
        }
    }

    private bool ShouldRebuild(IIndex index)
    {
        try
        {
            return !index.IndexExists() || (index is IIndexStats stats && stats.GetDocumentCount() == 0);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured trying to get determine index shouldRebuild status for index {IndexName}. The index will NOT be considered for rebuilding", index.Name);
            return false;
        }
    }

    private bool HasRegisteredPopulator(IIndex index) => _populators.Any(x => x.IsRegistered(index));
}
