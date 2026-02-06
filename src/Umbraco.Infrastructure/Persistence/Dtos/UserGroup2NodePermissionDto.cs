using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[Obsolete("Will be removed in Umbraco 18.")]
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal class UserGroup2NodePermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2NodePermission;
    public const string PrimaryKeyColumnName = "PK_umbracoUserGroup2NodePermission";

    private const string UserGroupIdColumnName = "userGroupId";
    private const string NodeIdColumnName = "nodeId";
    private const string PermissionColumnName = "permission";

    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = PrimaryKeyColumnName, OnColumns = $"{UserGroupIdColumnName}, {NodeIdColumnName}, {PermissionColumnName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUser2NodePermission_nodeId")]
    public int NodeId { get; set; }

    [Column(PermissionColumnName)]
    public string? Permission { get; set; }
}
