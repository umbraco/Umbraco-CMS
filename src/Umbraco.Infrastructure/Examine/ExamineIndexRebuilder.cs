// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal class ExamineIndexRebuilder : IIndexRebuilder
{
    // Static so that all instances of this type share the same rebuild task state across the application.
    private static readonly ConcurrentDictionary<string, Task> RebuildTasks = new();

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
        IEnumerable<IIndexPopulator> populators)
    {
        _mainDom = mainDom;
        _runtimeState = runtimeState;
        _logger = logger;
        _examineManager = examineManager;
        _populators = populators;
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
        => RebuildIndexAsync(indexName, delay, useBackgroundThread).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual async Task<Attempt<IndexRebuildResult>> RebuildIndexAsync(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        delay ??= TimeSpan.Zero;

        if (!CanRun())
        {
            return Attempt.Fail(IndexRebuildResult.NotAllowedToRun);
        }

        if (RebuildTasks.TryGetValue(indexName, out Task? existing) && !existing.IsCompleted)
        {
            return Attempt.Fail(IndexRebuildResult.AlreadyRebuilding);
        }

        if (useBackgroundThread)
        {
            // Do not flow AsyncLocal to the child thread
            using (ExecutionContext.SuppressFlow())
            {
                Task rebuildTask = Task.Run(() =>
                {
                    try
                    {
                        PerformRebuildIndex(indexName, delay.Value, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while rebuilding index {IndexName} in the background.", indexName);
                    }
                    finally
                    {
                        RebuildTasks.TryRemove(indexName, out _);
                    }
                });
                RebuildTasks[indexName] = rebuildTask;
            }
        }
        else
        {
            try
            {
                PerformRebuildIndex(indexName, delay.Value, CancellationToken.None);
            }
            finally
            {
                RebuildTasks.TryRemove(indexName, out _);
            }
        }

        return Attempt.Succeed(IndexRebuildResult.Success);
    }

    /// <inheritdoc/>
    [Obsolete("Use RebuildIndexesAsync() instead. Scheduled for removal in Umbraco 19.")]
    public virtual void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
        => RebuildIndexesAsync(onlyEmptyIndexes, delay, useBackgroundThread).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public virtual async Task<Attempt<IndexRebuildResult>> RebuildIndexesAsync(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        delay ??= TimeSpan.Zero;

        if (!CanRun())
        {
            return Attempt.Fail(IndexRebuildResult.NotAllowedToRun);
        }

        if (useBackgroundThread)
        {
            // Do not flow AsyncLocal to the child thread
            using (ExecutionContext.SuppressFlow())
            {
                _ = Task.Run(() =>
                {
                    PerformRebuildIndexes(onlyEmptyIndexes, delay.Value, CancellationToken.None);
                });
            }
        }
        else
        {
            PerformRebuildIndexes(onlyEmptyIndexes, delay.Value, CancellationToken.None);
        }

        return Attempt.Succeed(IndexRebuildResult.Success);
    }

    /// <inheritdoc/>
    public Task<bool> IsRebuildingAsync(string indexName)
        => Task.FromResult(RebuildTasks.TryGetValue(indexName, out Task? task) && task.IsCompleted is false);

    private bool CanRun() => _mainDom.IsMainDom && _runtimeState.Level == RuntimeLevel.Run;

    private void PerformRebuildIndex(string indexName, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            Thread.Sleep(delay);
        }

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

    private void PerformRebuildIndexes(bool onlyEmptyIndexes, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            Thread.Sleep(delay);
        }

        // If an index exists but it has zero docs we'll consider it empty and rebuild
        // Only include indexes that have at least one populator registered, this is to avoid emptying out indexes
        // that we have no chance of repopulating, for example our own search package
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
