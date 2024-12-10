using Umbraco.Cms.Api.Management.ServerEvents.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.Routing;

public class ServerEventSender : INotificationAsyncHandler<ContentSavedNotification>
{
    private readonly IServerEventRouter _serverEventRouter;

    public ServerEventSender(IServerEventRouter serverEventRouter)
    {
        _serverEventRouter = serverEventRouter;
    }

    public async Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent entity in notification.SavedEntities)
        {
            EventType eventType = EventType.Updated;
            if (entity.CreateDate == entity.UpdateDate)
            {
                // This is a new entity
                eventType = EventType.Created;
            }

            var eventModel = new ServerEvent()
            {
                EventType = eventType,
                Key = entity.Key,
                EventSource = EventSource.Document,
            };

            await _serverEventRouter.RouteEventAsync(eventModel);
        }
    }
}
