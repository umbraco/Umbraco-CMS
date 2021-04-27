using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerDeletingNotification : DeletingNotification<EntityContainer>
    {
        public EntityContainerDeletingNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
