using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[Obsolete("Scheduled for removal in Umbraco 18.")]
[TableName(TableName)]
[PrimaryKey([UserGroupIdColumnName, NodeIdColumnName, PermissionColumnName], AutoIncrement = false)]
[ExplicitColumns]
internal class UserGroup2NodePermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2NodePermission;

    private const string UserGroupIdColumnName = "userGroupId";
    private const string NodeIdColumnName = "nodeId";
    private const string PermissionColumnName = "permission";

    /// <summary>
    /// Gets or sets the unique identifier for the user group associated with the node permission.
    /// </summary>
    [Column(UserGroupIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_umbracoUserGroup2NodePermission", OnColumns = $"{UserGroupIdColumnName}, {NodeIdColumnName}, {PermissionColumnName}")]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the related node (such as a content or entity node) for which the permission applies.
    /// </summary>
    [Column(NodeIdColumnName)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUser2NodePermission_nodeId")]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the specific permission assigned to the user group for the associated node.
    /// </summary>
    [Column(PermissionColumnName)]
    public string? Permission { get; set; }
}
