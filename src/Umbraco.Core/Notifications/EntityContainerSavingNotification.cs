using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public class EntityContainerSavingNotification : SavingNotification<EntityContainer>
{
    public EntityContainerSavingNotification(EntityContainer target, EventMessages messages)
        : base(target, messages)
    {
    }
}
