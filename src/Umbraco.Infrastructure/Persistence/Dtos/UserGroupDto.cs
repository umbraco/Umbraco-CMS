using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    public UserGroupDto()
    {
        UserGroup2AppDtos = new List<UserGroup2AppDto>();
        UserGroup2LanguageDtos = new List<UserGroup2LanguageDto>();
        UserGroup2PermissionDtos = new List<UserGroup2PermissionDto>();
        UserGroup2GranularPermissionDtos = new List<UserGroup2GranularPermissionDto>();
    }

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 6)]
    public int Id { get; set; }

    [Column(KeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupKey")]
    public Guid Key { get; set; }

    [Column("userGroupAlias")]
    [Length(200)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupAlias")]
    public string? Alias { get; set; }

    [Column("userGroupName")]
    [Length(200)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupName")]
    public string? Name { get; set; }

    [Column(Name = "description")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Description { get; set; }

    [Column("userGroupDefaultPermissions")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Obsolete("Is not used anymore Use UserGroup2PermissionDtos instead. This will be removed in Umbraco 18.")]
    public string? DefaultPermissions { get; set; }

    [Column("createDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; }

    [Column("icon")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Icon { get; set; }

    [Column("hasAccessToAllLanguages")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public bool HasAccessToAllLanguages { get; set; }

    [Column("startContentId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto), Name = "FK_startContentId_umbracoNode_id")]
    public int? StartContentId { get; set; }

    [Column("startMediaId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto), Name = "FK_startMediaId_umbracoNode_id")]
    public int? StartMediaId { get; set; }

    [Column("startElementId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto), Name = "FK_startElementId_umbracoNode_id")]
    public int? StartElementId { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = UserGroup2AppDto.ReferenceMemberName)]
    public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = UserGroup2LanguageDto.ReferenceMemberName)]
    public List<UserGroup2LanguageDto> UserGroup2LanguageDtos { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
    public List<UserGroup2PermissionDto> UserGroup2PermissionDtos { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
    public List<UserGroup2GranularPermissionDto> UserGroup2GranularPermissionDtos { get; set; }

    /// <summary>
    ///     This is only relevant when this column is included in the results (i.e. GetUserGroupsWithUserCounts)
    /// </summary>
    [ResultColumn]
    public int UserCount { get; set; }
}
