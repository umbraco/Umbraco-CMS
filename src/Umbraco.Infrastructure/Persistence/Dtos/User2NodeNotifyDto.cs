using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([UserIdColumnName, NodeIdColumnName, ActionColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class User2NodeNotifyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ActionColumnName = "action";
    private const string UserIdColumnName = "userId";

    /// <summary>
    /// Gets or sets the identifier of the user associated with this notification.
    /// </summary>
    [Column(UserIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = $"{UserIdColumnName}, {NodeIdColumnName}, {ActionColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the node identifier.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the action type for the user-to-node notification.
    /// </summary>
    [Column(ActionColumnName)]
    [Length(255)]
    public string? Action { get; set; }
}
