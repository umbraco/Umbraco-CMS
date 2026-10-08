using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public class ElementEmptyingRecycleBinNotification : StatefulNotification, ICancelableNotification
{
    public ElementEmptyingRecycleBinNotification(EventMessages messages)
        => Messages = messages;

    public EventMessages Messages { get; }

    public bool Cancel { get; set; }
}
