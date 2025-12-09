using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerMovedNotification : MovedNotification<EntityContainer>
{
    public EntityContainerMovedNotification(MoveEventInfo<EntityContainer> target, EventMessages messages)
        : base(target, messages)
    {
    }
}
