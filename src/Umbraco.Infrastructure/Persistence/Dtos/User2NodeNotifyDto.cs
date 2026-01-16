using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class User2NodeNotifyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;
    public const string PrimaryKeyColumnName = "userId";
    public const string NodeIdColumnName = Constants.DatabaseSchema.Columns.NodeIdName;
    public const string ActionColumnName = "action";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = $"{PrimaryKeyColumnName}, {NodeIdColumnName}, {ActionColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column(ActionColumnName)]
    [Length(255)]
    public string? Action { get; set; }
}
