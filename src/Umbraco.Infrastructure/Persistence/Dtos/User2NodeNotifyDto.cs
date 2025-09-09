using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("userId", AutoIncrement = false)]
[ExplicitColumns]
internal sealed class User2NodeNotifyDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2NodeNotify;

    [Column("userId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column("action")]
    [Length(255)]
    public string? Action { get; set; }
}
