using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Represents a mapping between a user group and a permission in the database.
/// This DTO is used to associate user groups with their assigned permissions.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class UserGroup2PermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2Permission;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    private const string UserGroupKeyColumnName = "userGroupKey";
    private const string PermissionColumnName = "permission";

    /// <summary>
    /// Gets or sets the unique primary key identifier for this user group to permission mapping.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_userGroup2Permission", AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the key of the user group associated with this permission.
    /// </summary>
    [Column(UserGroupKeyColumnName)]
    [Index(IndexTypes.NonClustered, IncludeColumns = PermissionColumnName)]
    [ForeignKey(typeof(UserGroupDto), Column = UserGroupDto.KeyColumnName)]
    public Guid UserGroupKey { get; set; }

    /// <summary>
    /// Gets or sets the permission code assigned to the user group.
    /// This typically represents a specific action or access right.
    /// </summary>
    [Column(PermissionColumnName)]
    [Length(255)]
    public required string Permission { get; set; }
}
