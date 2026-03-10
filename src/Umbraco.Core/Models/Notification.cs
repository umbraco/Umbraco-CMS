namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a notification subscription for a user on an entity.
/// </summary>
public class Notification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Notification" /> class.
    /// </summary>
    /// <param name="entityId">The identifier of the entity being watched.</param>
    /// <param name="userId">The identifier of the user subscribing to the notification.</param>
    /// <param name="action">The action that triggers the notification.</param>
    /// <param name="entityType">The type of entity being watched.</param>
    public Notification(int entityId, int userId, string action, Guid entityType)
    {
        EntityId = entityId;
        UserId = userId;
        Action = action;
        EntityType = entityType;
    }

    /// <summary>
    ///     Gets the identifier of the entity being watched.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    ///     Gets the identifier of the user subscribing to the notification.
    /// </summary>
    public int UserId { get; }

    /// <summary>
    ///     Gets the action that triggers the notification.
    /// </summary>
    public string Action { get; }

    /// <summary>
    ///     Gets the type of entity being watched.
    /// </summary>
    public Guid EntityType { get; }
}
