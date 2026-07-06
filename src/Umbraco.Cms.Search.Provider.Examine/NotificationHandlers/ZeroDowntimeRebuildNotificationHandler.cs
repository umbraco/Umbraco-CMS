using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Cms.Search.Provider.Examine.NotificationHandlers;

// NOTE: This notification handler is only active when zero downtime reindexing is in effect
internal sealed class ZeroDowntimeRebuildNotificationHandler :
    INotificationHandler<IndexRebuildStartingNotification>,
    INotificationAsyncHandler<IndexRebuildCompletedNotification>
{
    private readonly IActiveIndexManager _activeIndexManager;
    private readonly IExamineManager _examineManager;
    private readonly IIndexCommitMonitor _indexCommitMonitor;
    private readonly ILogger<ZeroDowntimeRebuildNotificationHandler> _logger;

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

    public void Handle(IndexRebuildStartingNotification notification)
        => _activeIndexManager.StartRebuilding(notification.IndexAlias);

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
