using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerSavedNotification : SavedNotification<EntityContainer>
{
    public EntityContainerSavedNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
