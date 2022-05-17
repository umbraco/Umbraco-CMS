using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerDeletingNotification : DeletingNotification<EntityContainer>
{
    public EntityContainerDeletingNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
