using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerRenamedNotification : RenamedNotification<EntityContainer>
{
    public EntityContainerRenamedNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
