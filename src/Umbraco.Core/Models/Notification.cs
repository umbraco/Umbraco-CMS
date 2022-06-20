namespace Umbraco.Cms.Core.Models;

public class Notification
{
    public Notification(int entityId, int userId, string action, Guid? entityType)
    {
        EntityId = entityId;
        UserId = userId;
        Action = action;
        EntityType = entityType;
    }

    public int EntityId { get; }

    public int UserId { get; }

    public string Action { get; }

    public Guid? EntityType { get; }
}
