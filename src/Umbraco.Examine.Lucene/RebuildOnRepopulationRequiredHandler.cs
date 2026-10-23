using System.Runtime.CompilerServices;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Rebuilds an index when it reports that it has been cleared and needs to be repopulated.
/// </summary>
/// <seealso cref="UmbracoExamineIndex.RepopulationRequired" />
public sealed class RebuildOnRepopulationRequiredHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    // The notification is published again on restart, but each index only needs to be subscribed to once.
    private static readonly ConditionalWeakTable<UmbracoExamineIndex, object?> _subscribedIndexes = new();

    private readonly IExamineManager _examineManager;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ILogger<RebuildOnRepopulationRequiredHandler> _logger;

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
    {
        _examineManager = examineManager;
        _indexRebuilder = indexRebuilder;
        _logger = logger;
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

    private async Task RebuildAsync(string indexName)
    {
        try
        {
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
