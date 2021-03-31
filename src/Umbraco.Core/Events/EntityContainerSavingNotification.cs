using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Events
{
    public class EntityContainerSavingNotification : SavingNotification<EntityContainer>
    {
        public EntityContainerSavingNotification(EntityContainer target, EventMessages messages) : base(target, messages)
        {
        }
    }
}
