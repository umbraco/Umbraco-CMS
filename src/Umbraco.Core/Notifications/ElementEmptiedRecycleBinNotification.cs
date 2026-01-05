using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public class ElementEmptiedRecycleBinNotification : StatefulNotification
{
    public ElementEmptiedRecycleBinNotification(EventMessages messages)
        => Messages = messages;

    public EventMessages Messages { get; }

    public bool Cancel { get; set; }
}
