using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Api.Management.ViewModels.Events;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Api.Management.Routing;

public class ServerEventSender : INotificationHandler<ContentSavedNotification>
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;


    public ServerEventSender(IHubContext<ServerEventHub, IServerEventHub> eventHub)
    {
        _eventHub = eventHub;
    }

    public void Handle(ContentSavedNotification notification)
    {
        foreach (var entity in notification.SavedEntities)
        {
            var eventType = EventType.Updated;
            if (entity.CreateDate == entity.UpdateDate)
            {
                // This is a new entity
                eventType = EventType.Created;
            }
        }
    }
}
