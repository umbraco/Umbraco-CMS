using System.Runtime.CompilerServices;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

// TODO: Once https://github.com/Shazwazza/Examine/issues/629 is fixed, subscribe to the notification Examine raises
// instead of RepopulationRequired, or delete this handler if Examine no longer clears the index when recovering.

/// <summary>
///     Rebuilds an index when it reports that it has been cleared and needs to be repopulated.
/// </summary>
/// <seealso cref="UmbracoExamineIndex.RepopulationRequired" />
internal sealed class RebuildOnRepopulationRequiredHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    // The notification is published again on restart, but each index only needs to be subscribed to once.
    private static readonly ConditionalWeakTable<UmbracoExamineIndex, object?> _subscribedIndexes = new();

    private static readonly TimeSpan _rebuildInProgressPollInterval = TimeSpan.FromSeconds(30);

    private readonly IExamineManager _examineManager;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<RebuildOnRepopulationRequiredHandler> _logger;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RebuildOnRepopulationRequiredHandler" /> class.
    /// </summary>
    /// <param name="examineManager">The manager providing the indexes to subscribe to.</param>
    /// <param name="indexRebuilder">The service responsible for rebuilding Examine indexes.</param>
    /// <param name="logger">The logger.</param>
    public RebuildOnRepopulationRequiredHandler(
        IExamineManager examineManager,
        IIndexRebuilder indexRebuilder,
        ILogger<RebuildOnRepopulationRequiredHandler> logger)
        : this(examineManager, indexRebuilder, logger, TimeProvider.System)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RebuildOnRepopulationRequiredHandler" /> class using the
    ///     specified time provider.
    /// </summary>
    /// <param name="examineManager">The manager providing the indexes to subscribe to.</param>
    /// <param name="indexRebuilder">The service responsible for rebuilding Examine indexes.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="timeProvider">The time provider used while waiting for a rebuild in progress to finish.</param>
    /// <remarks>
    ///     This constructor is internal because the time provider only exists so tests can control the wait. Production
    ///     code uses <see cref="TimeProvider.System" /> via the public constructor.
    /// </remarks>
    internal RebuildOnRepopulationRequiredHandler(
        IExamineManager examineManager,
        IIndexRebuilder indexRebuilder,
        ILogger<RebuildOnRepopulationRequiredHandler> logger,
        TimeProvider timeProvider)
    {
        _examineManager = examineManager;
        _indexRebuilder = indexRebuilder;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    /// <summary>
    ///     Subscribes to the indexes that can report that they need to be repopulated.
    /// </summary>
    /// <param name="notification">The notification.</param>
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        foreach (UmbracoExamineIndex index in _examineManager.Indexes.OfType<UmbracoExamineIndex>())
        {
            if (_subscribedIndexes.TryAdd(index, null))
            {
                index.RepopulationRequired += OnRepopulationRequired;
            }
        }
    }

    private void OnRepopulationRequired(object? sender, EventArgs e)
    {
        if (sender is IIndex index)
        {
            _ = RebuildAsync(index.Name);
        }
    }

    /// <summary>
    ///     Requests a rebuild of the index, once any rebuild of it that is in progress has finished.
    /// </summary>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <returns>A task that completes when the rebuild has been requested.</returns>
    internal async Task RebuildAsync(string indexName)
    {
        try
        {
            // A rebuild in progress has lost the work it did before the index was cleared, so it cannot complete the
            // index. Rebuild again once it has finished, as a rebuild requested while one is running may be ignored.
            while (await _indexRebuilder.IsRebuildingAsync(indexName))
            {
                await Task.Delay(_rebuildInProgressPollInterval, _timeProvider);
            }

            Attempt<IndexRebuildResult> result = await _indexRebuilder.RebuildIndexAsync(indexName);
            if (result.Success is false)
            {
                _logger.LogWarning("The index {IndexName} needs to be repopulated but could not be rebuilt: {Result}", indexName, result.Result);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The index {IndexName} needs to be repopulated but rebuilding it failed.", indexName);
        }
    }
}
