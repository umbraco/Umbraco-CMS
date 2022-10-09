using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.User2NodeNotify)]
[PrimaryKey("userId", AutoIncrement = false)]
[ExplicitColumns]
internal class User2NodeNotifyDto
{
    [Column("userId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUser2NodeNotify", OnColumns = "userId, nodeId, action")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto))]
    public int NodeId { get; set; }

    [Column("action")]
    [SpecialDbType(SpecialDbTypes.NCHAR)]
    [Length(1)]
    public string? Action { get; set; }
}
