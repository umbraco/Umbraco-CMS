using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerRenamedNotification : RenamedNotification<EntityContainer>
    {
        public EntityContainerRenamedNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
