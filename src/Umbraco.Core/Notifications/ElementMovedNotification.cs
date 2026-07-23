using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// The notification is published and called after the element has been moved.
/// </summary>
public sealed class ElementMovedNotification : MovedNotification<IElement>
{
    public ElementMovedNotification(MoveEventInfo<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
