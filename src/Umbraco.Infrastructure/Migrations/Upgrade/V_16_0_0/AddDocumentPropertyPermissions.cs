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
    /// <summary>
    /// Initializes a new instance of the <see cref="AddDocumentPropertyPermissions"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The migration context to use for this migration.</param>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroupDto"/> class used in the document property permissions migration.
        /// </summary>
        public UserGroupDto()
        {
            UserGroup2AppDtos = [];
            UserGroup2LanguageDtos = [];
            UserGroup2PermissionDtos = [];
            UserGroup2GranularPermissionDtos = [];
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user group.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 6)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique key that identifies the user group.
        /// </summary>
        [Column("key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupKey")]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the unique alias of the user group.
        /// </summary>
        [Column("userGroupAlias")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupAlias")]
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of this user group.
        /// </summary>
        [Column("userGroupName")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupName")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the default permissions for the user group.
        /// This property is obsolete and no longer used. Use <see cref="UserGroup2PermissionDtos"/> instead.
        /// Scheduled for removal in Umbraco 18.
        /// </summary>
        [Column("userGroupDefaultPermissions")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Obsolete("Is not used anymore. Use UserGroup2PermissionDtos instead. Scheduled for removal in Umbraco 18.")]
        public string? DefaultPermissions { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user group was created.
        /// </summary>
        [Column("createDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user group was last updated.
        /// </summary>
        [Column("updateDate")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Gets or sets the icon for the user group.
        /// </summary>
        [Column("icon")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user group has access to all languages.
        /// </summary>
        [Column("hasAccessToAllLanguages")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool HasAccessToAllLanguages { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the start content node assigned to the user group.
        /// </summary>
        [Column("startContentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(NodeDto), Name = "FK_startContentId_umbracoNode_id")]
        public int? StartContentId { get; set; }

        /// <summary>Gets or sets the start media identifier for the user group.</summary>
        [Column("startMediaId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(NodeDto), Name = "FK_startMediaId_umbracoNode_id")]
        public int? StartMediaId { get; set; }

        /// <summary>
        /// Gets or sets the collection of application associations for this user group.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

        /// <summary>
        /// Gets or sets the collection of language associations for this user group.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2LanguageDto> UserGroup2LanguageDtos { get; set; }

        /// <summary>
        /// Gets or sets the list of document property permissions associated with the user group.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2PermissionDto> UserGroup2PermissionDtos { get; set; }

        /// <summary>
        /// Gets or sets the collection of granular permission mappings associated with this user group.
        /// Each item represents a specific granular permission assigned to the group.
        /// </summary>
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
