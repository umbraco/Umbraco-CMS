using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerRenamingNotification : RenamingNotification<EntityContainer>
{
    public EntityContainerRenamingNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
