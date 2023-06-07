using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.UserGroup)]
[PrimaryKey("id")]
[ExplicitColumns]
public class UserGroupDto
{
    public UserGroupDto()
    {
        UserGroup2AppDtos = new List<UserGroup2AppDto>();
        UserGroup2LanguageDtos = new List<UserGroup2LanguageDto>();
    }

    [Column("id")]
    [PrimaryKeyColumn(IdentitySeed = 6)]
    public int Id { get; set; }

    [Column("userGroupAlias")]
    [Length(200)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupAlias")]
    public string? Alias { get; set; }

    [Column("userGroupName")]
    [Length(200)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupName")]
    public string? Name { get; set; }

    [Column("userGroupDefaultPermissions")]
    [Length(50)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? DefaultPermissions { get; set; }

    [Column("createDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; }

    [Column("updateDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
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

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
    public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
    public List<UserGroup2LanguageDto> UserGroup2LanguageDtos { get; set; }

    /// <summary>
    ///     This is only relevant when this column is included in the results (i.e. GetUserGroupsWithUserCounts)
    /// </summary>
    [ResultColumn]
    public int UserCount { get; set; }
}
