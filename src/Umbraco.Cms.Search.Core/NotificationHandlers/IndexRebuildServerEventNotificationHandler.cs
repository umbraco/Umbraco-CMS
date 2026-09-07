using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Search.Core.Notifications;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Notification handler that broadcasts SignalR server events when index rebuilds start and complete.
/// </summary>
internal sealed class IndexRebuildServerEventNotificationHandler :
    INotificationAsyncHandler<IndexRebuildStartingNotification>,
    INotificationAsyncHandler<IndexRebuildCompletedNotification>
{
    private readonly IServerEventRouter _serverEventRouter;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexRebuildServerEventNotificationHandler"/> class.
    /// </summary>
    /// <param name="serverEventRouter">The router used to broadcast server events to the backoffice UI.</param>
    public IndexRebuildServerEventNotificationHandler(IServerEventRouter serverEventRouter)
    {
        _serverEventRouter = serverEventRouter;
    }

    /// <summary>
    /// Broadcasts an "IndexRebuildStarting" server event for the index that is about to be rebuilt.
    /// </summary>
    /// <param name="notification">The notification describing the index rebuild that is starting.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task HandleAsync(IndexRebuildStartingNotification notification, CancellationToken cancellationToken)
    {
        await _serverEventRouter.BroadcastEventAsync(new ServerEvent
        {
            EventType = "IndexRebuildStarting",
            EventSource = notification.IndexAlias,
        });
    }

    /// <summary>
    /// Broadcasts an "IndexRebuildCompleted" server event for the index that finished rebuilding.
    /// </summary>
    /// <param name="notification">The notification describing the index rebuild that completed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task HandleAsync(IndexRebuildCompletedNotification notification, CancellationToken cancellationToken)
    {
        await _serverEventRouter.BroadcastEventAsync(new ServerEvent
        {
            EventType = "IndexRebuildCompleted",
            EventSource = notification.IndexAlias,
        });
    }
}
