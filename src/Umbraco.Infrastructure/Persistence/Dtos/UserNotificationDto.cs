using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal sealed class UserNotificationDto
{
    /// <summary>
    /// Gets or sets the identifier of the node related to this user notification.
    /// </summary>
    [Column("nodeId")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user associated with the notification.
    /// </summary>
    [Column("userId")]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) representing the object type of the related node.
    /// </summary>
    [Column("nodeObjectType")]
    public Guid NodeObjectType { get; set; }

    /// <summary>
    /// Gets or sets the name of the action that triggered the user notification.
    /// </summary>
    [Column("action")]
    public string Action { get; set; } = null!;
}
