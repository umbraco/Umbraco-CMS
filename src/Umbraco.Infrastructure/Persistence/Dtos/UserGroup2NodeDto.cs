using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
internal class UserGroup2NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2Node;

    [Column("userGroupId")]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_" + TableName, OnColumns = "userGroupId, nodeId")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_nodeId")]
    public int NodeId { get; set; }
}
