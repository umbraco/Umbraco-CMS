using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerRenamingNotification : RenamingNotification<EntityContainer>
    {
        public EntityContainerRenamingNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
