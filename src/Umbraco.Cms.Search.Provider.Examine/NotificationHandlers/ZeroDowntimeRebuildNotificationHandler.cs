using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Cms.Search.Provider.Examine.NotificationHandlers;

// NOTE: This notification handler is only active when zero downtime reindexing is in effect
/// <summary>
/// Implements the active/shadow index swap for zero-downtime rebuilds: marks an index as rebuilding when it starts,
/// then waits for the shadow index to commit and verifies it is healthy before swapping it in, cancelling the swap otherwise.
/// </summary>
internal sealed class ZeroDowntimeRebuildNotificationHandler :
    INotificationHandler<IndexRebuildStartingNotification>,
    INotificationAsyncHandler<IndexRebuildCompletedNotification>
{
    private readonly IActiveIndexManager _activeIndexManager;
    private readonly IExamineManager _examineManager;
    private readonly IIndexCommitMonitor _indexCommitMonitor;
    private readonly ILogger<ZeroDowntimeRebuildNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZeroDowntimeRebuildNotificationHandler"/> class.
    /// </summary>
    /// <param name="activeIndexManager">The manager used to track and swap the active/shadow index slots.</param>
    /// <param name="examineManager">The manager used to check the shadow index's existence and document count.</param>
    /// <param name="indexCommitMonitor">The monitor used to wait for the shadow index's Lucene commit before swapping.</param>
    /// <param name="logger">The logger used to record the outcome of the shadow index swap.</param>
    public ZeroDowntimeRebuildNotificationHandler(
        IActiveIndexManager activeIndexManager,
        IExamineManager examineManager,
        IIndexCommitMonitor indexCommitMonitor,
        ILogger<ZeroDowntimeRebuildNotificationHandler> logger)
    {
        _activeIndexManager = activeIndexManager;
        _examineManager = examineManager;
        _indexCommitMonitor = indexCommitMonitor;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Handle(IndexRebuildStartingNotification notification)
        => _activeIndexManager.StartRebuilding(notification.IndexAlias);

    /// <inheritdoc />
    public async Task HandleAsync(IndexRebuildCompletedNotification notification, CancellationToken cancellationToken)
    {
        var shadowIndexName = _activeIndexManager.ResolveShadowIndexName(notification.IndexAlias);

        // Examine's LuceneIndex.IndexItems() commits asynchronously. We must wait for the
        // commit to complete before checking document count, otherwise we'll see 0 documents
        // and incorrectly cancel the swap.
        var committed = await _indexCommitMonitor.WaitForCommitAsync(shadowIndexName, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation requested before completion of shadow index swap for {ShadowIndex}.", shadowIndexName);
            return;
        }

        if (committed is false)
        {
            _logger.LogWarning("Timed out waiting for shadow index {ShadowIndex} to commit after rebuild", shadowIndexName);
        }

        if (committed && IsShadowIndexHealthy(shadowIndexName))
        {
            _activeIndexManager.CompleteRebuilding(notification.IndexAlias);
            ClearShadowIndex(notification.IndexAlias);
        }
        else
        {
            _logger.LogWarning(
                "Shadow index {ShadowIndex} is empty or unhealthy after rebuild of {IndexAlias}. Cancelling swap.",
                shadowIndexName,
                notification.IndexAlias);
            _activeIndexManager.CancelRebuilding(notification.IndexAlias);
        }
    }

    private void ClearShadowIndex(string indexAlias)
    {
        var shadowIndexName = _activeIndexManager.ResolveShadowIndexName(indexAlias);

        if (_examineManager.TryGetIndex(shadowIndexName, out IIndex? index) is false)
        {
            return;
        }

        _logger.LogInformation("Clearing shadow index {ShadowIndex} after successful swap for {IndexAlias}.", shadowIndexName, indexAlias);
        index.CreateIndex();
    }

    private bool IsShadowIndexHealthy(string physicalIndexName)
    {
        if (_examineManager.TryGetIndex(physicalIndexName, out IIndex? index) is false)
        {
            return false;
        }

        if (index.IndexExists() is false)
        {
            return false;
        }

        if (index is IIndexStats stats && stats.GetDocumentCount() > 0)
        {
            return true;
        }

        return false;
    }
}
