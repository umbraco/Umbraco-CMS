using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(UserStartNodeDtoConfiguration))]
public class UserStartNodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserStartNode;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier of the user start node.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user associated with this start node.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the start node identifier for the user.
    /// </summary>
    public int StartNode { get; set; }

    /// <summary>
    /// Gets or sets the type of the start node (1 = Content, 2 = Media, 3 = Element).
    /// </summary>
    public int StartNodeType { get; set; }
}
