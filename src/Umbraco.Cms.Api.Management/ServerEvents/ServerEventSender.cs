using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Api.Management.ServerEvents.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
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
                SourceType = entity.GetType().Name,
            };

            _eventHub.Clients.All.notify(eventModel);
            // foreach (var VARIABLE in _eventHub.Clients.Users())
            // {
            //
            // }
        }
    }
}
