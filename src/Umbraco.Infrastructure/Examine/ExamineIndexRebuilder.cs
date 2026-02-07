// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal class ExamineIndexRebuilder : IIndexRebuilder
{
    private const string RebuildAllOperationTypeName = "RebuildAllExamineIndexes";

    private readonly IExamineManager _examineManager;
    private readonly ILogger<ExamineIndexRebuilder> _logger;
    private readonly IMainDom _mainDom;
    private readonly IEnumerable<IIndexPopulator> _populators;
    private readonly ILongRunningOperationService _longRunningOperationService;
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
        ILongRunningOperationService longRunningOperationService)
    {
        _mainDom = mainDom;
        _runtimeState = runtimeState;
        _logger = logger;
        _examineManager = examineManager;
        _populators = populators;
        _longRunningOperationService = longRunningOperationService;
    }

    /// <inheritdoc/>
    public bool CanRebuild(string indexName)
    {
        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            throw new InvalidOperationException("No index found by name " + indexName);
        }

        return HasRegisteredPopulator(index);
    }

    /// <inheritdoc/>
    [Obsolete("Use RebuildIndexAsync() instead. Scheduled for removal in v19.")]
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

        Attempt<Guid, LongRunningOperationEnqueueStatus> attempt = await _longRunningOperationService.RunAsync(
            GetRebuildOperationTypeName(indexName),
            async ct =>
            {
                await RebuildIndex(indexName, delay.Value, ct);
                return Task.CompletedTask;
            },
            allowConcurrentExecution: false,
            runInBackground: useBackgroundThread);

        if (attempt.Success)
        {
            return Attempt.Succeed(IndexRebuildResult.Success);
        }

        return attempt.Status switch
        {
            LongRunningOperationEnqueueStatus.AlreadyRunning => Attempt.Fail(IndexRebuildResult.AlreadyRebuilding),
            _ => Attempt.Fail(IndexRebuildResult.Unknown),
        };
    }

    /// <inheritdoc/>
    [Obsolete("Use RebuildIndexesAsync() instead. Scheduled for removal in v19.")]
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

        Attempt<Guid, LongRunningOperationEnqueueStatus> attempt = await _longRunningOperationService.RunAsync(
            RebuildAllOperationTypeName,
            async ct =>
            {
                await RebuildIndexes(onlyEmptyIndexes, delay.Value, ct);
                return Task.CompletedTask;
            },
            allowConcurrentExecution: false,
            runInBackground: useBackgroundThread);

        if (attempt.Success)
        {
            return Attempt.Succeed(IndexRebuildResult.Success);
        }

        return attempt.Status switch
        {
            LongRunningOperationEnqueueStatus.AlreadyRunning => Attempt.Fail(IndexRebuildResult.AlreadyRebuilding),
            _ => Attempt.Fail(IndexRebuildResult.Unknown),
        };
    }

    /// <inheritdoc/>
    public async Task<bool> IsRebuildingAsync(string indexName)
        => (await _longRunningOperationService.GetByTypeAsync(GetRebuildOperationTypeName(indexName), 0, 0)).Total != 0;

    private static string GetRebuildOperationTypeName(string indexName) => $"RebuildExamineIndex-{indexName}";

    private bool CanRun() => _mainDom.IsMainDom && _runtimeState.Level == RuntimeLevel.Run;

    private async Task RebuildIndex(string indexName, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
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

    private async Task RebuildIndexes(bool onlyEmptyIndexes, TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
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
