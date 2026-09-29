using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(User2NodeNotifyDtoConfiguration))]
public class User2NodeNotifyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;

    /// <summary>
    /// Gets or sets the identifier of the user associated with this notification.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the node identifier.
    /// </summary>
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the action type for the user-to-node notification.
    /// </summary>
    public string? Action { get; set; }
}
