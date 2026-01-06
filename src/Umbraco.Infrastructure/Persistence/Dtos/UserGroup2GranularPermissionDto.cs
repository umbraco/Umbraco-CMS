using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName, AutoIncrement = true)]
[ExplicitColumns]
public class UserGroup2GranularPermissionDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup2GranularPermission;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNameId;
    public const string UniqueIdName = Constants.DatabaseSchema.UniqueIdName;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn(Name = "PK_umbracoUserGroup2GranularPermissionDto", AutoIncrement = true)]
    public int Id { get; set; }

    [Column("userGroupKey")]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UserGroupKey_UniqueId", IncludeColumns = UniqueIdName)]
    [ForeignKey(typeof(UserGroupDto), Column = UserGroupDto.KeyColumnName)]
    public Guid UserGroupKey { get; set; }

    [Column(UniqueIdName)]
    [ForeignKey(typeof(NodeDto), Column = UniqueIdName)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Index(IndexTypes.NonClustered, Name = "IX_umbracoUserGroup2GranularPermissionDto_UniqueId")]
    public Guid? UniqueId { get; set; }

    [Column("permission")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Permission { get; set; }

    [Column("context")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public required string Context { get; set; }
}


// this is UserGroup2GranularPermissionDto + int ids
// it is used for handling legacy cases where we use int Ids
internal sealed class UserGroup2GranularPermissionWithIdsDto : UserGroup2GranularPermissionDto
{
    [Column("entityId")]
    public int EntityId { get; set; }

    [Column("userGroupId")]
    public int UserGroupId { get; set; }
}
