using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_0_0;

[Obsolete("Remove in Umbraco 18.")]
internal class AddDocumentPropertyPermissions : MigrationBase
{
    public AddDocumentPropertyPermissions(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        List<UserGroupDto>? userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroupDto in userGroups ?? [])
        {
            List<UserGroup2PermissionDto>? currentPermissions = Database.Fetch<UserGroup2PermissionDto>(
                "WHERE userGroupKey = @UserGroupKey",
                new { UserGroupKey = userGroupDto.Key });

            if (currentPermissions is null || currentPermissions.Count is 0)
            {
                continue;
            }

            if (currentPermissions.Any(p => p.Permission == ActionDocumentPropertyRead.ActionLetter))
            {
                return;
            }

            Database.InsertBulk(
            [
                new UserGroup2PermissionDto
                {
                    UserGroupKey = userGroupDto.Key, Permission = ActionDocumentPropertyRead.ActionLetter
                },
                new UserGroup2PermissionDto
                {
                    UserGroupKey = userGroupDto.Key, Permission = ActionDocumentPropertyWrite.ActionLetter
                }
            ]);
        }
    }

    /// <summary>
    /// Represents the UserGroupDto at the point of this migration step.
    /// </summary>
    /// <remarks>
    /// Note that this isn't the same as <see cref="Persistence.Dtos.UserGroupDto"/>, as this has had further
    /// changes since this migration was designed (e.g. the addition of Description). If we use that directly we'll
    /// get exceptions in this migration step on updates.
    /// </remarks>
    [TableName(Constants.DatabaseSchema.Tables.UserGroup)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    private class UserGroupDto
    {
        public UserGroupDto()
        {
            UserGroup2AppDtos = [];
            UserGroup2LanguageDtos = [];
            UserGroup2PermissionDtos = [];
            UserGroup2GranularPermissionDtos = [];
        }

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 6)]
        public int Id { get; set; }

        [Column("key")]
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

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
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
}
