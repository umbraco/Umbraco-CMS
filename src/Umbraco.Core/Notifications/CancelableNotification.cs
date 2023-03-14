using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Core.Notifications;

public class CancelableNotification : StatefulNotification, ICancelableNotification
{
    public CancelableNotification(EventMessages messages) => Messages = messages;

    public EventMessages Messages { get; }

    public bool Cancel { get; set; }

    public void CancelOperation(EventMessage cancellationMessage)
    {
        Cancel = true;
        cancellationMessage.IsDefaultEventMessage = true;
        Messages.Add(cancellationMessage);
    }
}
