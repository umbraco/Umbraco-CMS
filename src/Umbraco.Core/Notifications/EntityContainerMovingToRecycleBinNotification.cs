using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerMovingToRecycleBinNotification : MovingToRecycleBinNotification<EntityContainer>
{
    public EntityContainerMovingToRecycleBinNotification(MoveToRecycleBinEventInfo<EntityContainer> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
