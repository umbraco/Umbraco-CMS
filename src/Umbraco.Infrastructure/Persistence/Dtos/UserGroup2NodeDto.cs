using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[Obsolete("Will be removed in Umbraco 18.")]
[TableName(TableName)]
[PrimaryKey([UserGroupIdColumnName, NodeIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal sealed class UserGroup2NodeDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2Node;
    public const string PrimaryKeyColumnName = "PK_" + TableName;

    private const string UserGroupIdColumnName = "userGroupId";
    private const string NodeIdColumnName = "nodeId";

    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = PrimaryKeyColumnName, OnColumns = $"{UserGroupIdColumnName}, {NodeIdColumnName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_nodeId")]
    public int NodeId { get; set; }
}
