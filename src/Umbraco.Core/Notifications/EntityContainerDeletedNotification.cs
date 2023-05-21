using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerDeletedNotification : DeletedNotification<EntityContainer>
{
    public EntityContainerDeletedNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
