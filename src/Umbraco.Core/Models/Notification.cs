using System;

namespace Umbraco.Cms.Core.Models
{
    public class Notification
    {
        public Notification(int entityId, int userId, string action, Guid? entityType)
        {
            EntityId = entityId;
            UserId = userId;
            Action = action;
            EntityType = entityType;
        }

        public int EntityId { get; private set; }
        public int UserId { get; private set; }
        public string Action { get; private set; }
        public Guid? EntityType { get; private set; }
    }
}
