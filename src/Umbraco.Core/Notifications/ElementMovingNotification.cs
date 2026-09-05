using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// Called while an element is moving, but before it has been moved. Cancel the operation to prevent the movement.
/// </summary>
public sealed class ElementMovingNotification : MovingNotification<IElement>
{
    public ElementMovingNotification(MoveEventInfo<IElement> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
