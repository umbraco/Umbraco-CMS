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

    public IndexRebuildServerEventNotificationHandler(IServerEventRouter serverEventRouter)
    {
        _serverEventRouter = serverEventRouter;
    }

    public async Task HandleAsync(IndexRebuildStartingNotification notification, CancellationToken cancellationToken)
    {
        await _serverEventRouter.BroadcastEventAsync(new ServerEvent
        {
            EventType = "IndexRebuildStarting",
            EventSource = notification.IndexAlias,
        });
    }

    public async Task HandleAsync(IndexRebuildCompletedNotification notification, CancellationToken cancellationToken)
    {
        await _serverEventRouter.BroadcastEventAsync(new ServerEvent
        {
            EventType = "IndexRebuildCompleted",
            EventSource = notification.IndexAlias,
        });
    }
}
