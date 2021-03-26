using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerRenamedNotification : SavedNotification<EntityContainer>
    {
        public EntityContainerRenamedNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
