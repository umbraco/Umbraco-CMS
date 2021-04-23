using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerSavedNotification : SavedNotification<EntityContainer>
    {
        public EntityContainerSavedNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
