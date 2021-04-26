using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerDeletedNotification : DeletedNotification<EntityContainer>
    {
        public EntityContainerDeletedNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
