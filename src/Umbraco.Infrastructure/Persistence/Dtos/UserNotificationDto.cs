using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

internal class UserNotificationDto
{
    [Column("nodeId")]
    public int NodeId { get; set; }

    [Column("userId")]
    public int UserId { get; set; }

    [Column("nodeObjectType")]
    public Guid NodeObjectType { get; set; }

    [Column("action")]
    public string Action { get; set; } = null!;
}
