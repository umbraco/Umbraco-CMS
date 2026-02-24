using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(User2NodeNotifyDtoConfiguration))]
public sealed class User2NodeNotifyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;
    public const string UserIdColumnName = "userId";
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ActionColumnName = "action";

    public int UserId { get; set; }

    public int NodeId { get; set; }

    public string Action { get; set; } = string.Empty;

    // Navigation to NodeDto (in EFCore model)
    public NodeDto NodeDto { get; set; } = null!;

    // TODO: Add navigation to UserDto when UserDto is migrated to EFCore
}
