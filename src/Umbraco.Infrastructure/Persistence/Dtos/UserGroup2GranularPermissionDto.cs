using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents an association between a user group and its assigned granular permissions.
/// This DTO is used to map specific permissions to user groups within the system.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class UserGroup2GranularPermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2GranularPermission;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string UniqueIdColumnName = Constants.DatabaseSchema.Columns.UniqueIdName;

    private const string UserGroupKeyColumnName = "userGroupKey";

    /// <summary>
    /// Gets or sets the primary key identifier for this user group to granular permission mapping.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_umbracoUserGroup2GranularPermissionDto", AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the user group.
    /// </summary>
    [Column(UserGroupKeyColumnName)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UserGroupKey_UniqueId", IncludeColumns = UniqueIdColumnName)]
    [ForeignKey(typeof(UserGroupDto), Column = UserGroupDto.KeyColumnName)]
    public Guid UserGroupKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier (GUID) for the associated user group granular permission.
    /// </summary>
    [Column(UniqueIdColumnName)]
    [ForeignKey(typeof(NodeDto), Column = NodeDto.KeyColumnName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UniqueId")]
    public Guid? UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the granular permission assigned to the user group.
    /// </summary>
    [Column("permission")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Permission { get; set; }

    /// <summary>
    /// Gets or sets the context in which the granular permission applies for the user group.
    /// </summary>
    [Column("context")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Context { get; set; }
}


// this is UserGroup2GranularPermissionDto + int ids
// it is used for handling legacy cases where we use int Ids
internal sealed class UserGroup2GranularPermissionWithIdsDto : UserGroup2GranularPermissionDto
{
    /// <summary>
    /// Gets or sets the identifier of the entity to which the granular permission applies.
    /// </summary>
    [Column("entityId")]
    public int EntityId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user group.
    /// </summary>
    [Column("userGroupId")]
    public int UserGroupId { get; set; }
}
