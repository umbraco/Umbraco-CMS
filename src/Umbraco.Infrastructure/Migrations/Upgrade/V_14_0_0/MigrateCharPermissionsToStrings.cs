using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

[Obsolete("Remove in Umbraco 18.")]
internal class MigrateCharPermissionsToStrings : MigrationBase
{
    private readonly IIdKeyMap _idKeyMap;

    internal static Dictionary<char, IEnumerable<string>> CharToStringPermissionDictionary { get; } =
        new()
        {
            ['I'] = new []{ActionAssignDomain.ActionLetter},
            ['F'] = new []{ActionBrowse.ActionLetter},
            ['O'] = new []{ActionCopy.ActionLetter},
            ['ï'] = new []{ActionCreateBlueprintFromContent.ActionLetter},
            ['D'] = new []{ActionDelete.ActionLetter},
            ['M'] = new []{ActionMove.ActionLetter},
            ['C'] = new []{ActionNew.ActionLetter},
            ['N'] = new []{ActionNotify.ActionLetter},
            ['P'] = new []{ActionProtect.ActionLetter},
            ['U'] = new []{ActionPublish.ActionLetter},
            ['V'] = new []{ActionRestore.ActionLetter},
            ['R'] = new []{ActionRights.ActionLetter},
            ['K'] = new []{ActionRollback.ActionLetter},
            ['S'] = new []{ActionSort.ActionLetter},
            ['Z'] = new []{ActionUnpublish.ActionLetter},
            ['A'] = new []{ActionUpdate.ActionLetter},
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateCharPermissionsToStrings"/> class, which migrates character-based permissions to string-based permissions during the upgrade process.
    /// </summary>
    /// <param name="context">The migration context used to manage the migration process.</param>
    /// <param name="idKeyMap">The mapping service for converting between integer IDs and GUID keys.</param>
    public MigrateCharPermissionsToStrings(IMigrationContext context, IIdKeyMap idKeyMap)
        : base(context)
    {
        _idKeyMap = idKeyMap;
    }

    protected override void Migrate()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission))
        {
            return;
        }

        List<UserGroupDto>? userGroups = Database.Fetch<UserGroupDto>();

        foreach (UserGroupDto userGroupDto in userGroups)
        {
            if (userGroupDto.DefaultPermissions == null)
            {
                continue;
            }

            var permissions = userGroupDto.DefaultPermissions.SelectMany(oldPermission =>
            {
                IEnumerable<string> newPermissions = ReplacePermissionValue(oldPermission);
                return newPermissions.Select(permission => new UserGroup2PermissionDto()
                {
                    Permission = permission, UserGroupKey = userGroupDto.Key
                });
            }).ToHashSet();

            Database.InsertBulk(permissions);

            userGroupDto.DefaultPermissions = null;

            Database.Update(userGroupDto);
        }

        Create.Table<UserGroup2GranularPermissionDto>().Do();


        List<UserGroup2NodePermissionDto>? userGroup2NodePermissionDtos = Database.Fetch<UserGroup2NodePermissionDto>();

        var userGroupIdToKeys = userGroups.ToDictionary(x => x.Id, x => x.Key);
        IEnumerable<UserGroup2GranularPermissionDto> userGroup2GranularPermissionDtos = userGroup2NodePermissionDtos.SelectMany(userGroup2NodePermissionDto =>
        {
            HashSet<string> permissions = userGroup2NodePermissionDto.Permission?.SelectMany(ReplacePermissionValue).ToHashSet() ?? new HashSet<string>();

            return permissions.Select(permission =>
            {
                var uniqueIdAttempt =
                    _idKeyMap.GetKeyForId(userGroup2NodePermissionDto.NodeId, UmbracoObjectTypes.Document);

                if (uniqueIdAttempt.Success is false)
                {
                    throw new InvalidOperationException("Did not find a key for the document id: " +
                                                        userGroup2NodePermissionDto.NodeId);
                }

                return new UserGroup2GranularPermissionDto()
                {
                    Permission = permission,
                    UserGroupKey = userGroupIdToKeys[userGroup2NodePermissionDto.UserGroupId],
                    UniqueId = uniqueIdAttempt.Result,
                    Context = DocumentGranularPermission.ContextType
                };
            });
        });

        Database.InsertBulk(userGroup2GranularPermissionDtos);

        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup2NodePermission).Do();
        Delete.Table(Constants.DatabaseSchema.Tables.UserGroup2Node).Do();
    }

    private IEnumerable<string> ReplacePermissionValue(char oldPermission) => CharToStringPermissionDictionary.TryGetValue(oldPermission, out IEnumerable<string>? newPermission) ? newPermission : oldPermission.ToString().Yield();

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
        /// Initializes a new instance of the <see cref="UserGroupDto"/> class with default values.
        /// </summary>
        public UserGroupDto()
        {
            UserGroup2AppDtos = [];
            UserGroup2LanguageDtos = [];
            UserGroup2PermissionDtos = [];
            UserGroup2GranularPermissionDtos = [];
        }

        /// <summary>Gets or sets the unique identifier for the user group.</summary>
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 6)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the user group.
        /// </summary>
        [Column("key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Constraint(Default = SystemMethods.NewGuid)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupKey")]
        public Guid Key { get; set; }

        /// <summary>Gets or sets the alias of the user group.</summary>
        [Column("userGroupAlias")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupAlias")]
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the user group.
        /// </summary>
        [Column("userGroupName")]
        [Length(200)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupName")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the default permissions for the user group as a string.
        /// <para>
        /// <b>Obsolete:</b> This property is no longer used. Use <see cref="UserGroup2PermissionDtos"/> instead. Scheduled for removal in Umbraco 18.
        /// </para>
        /// </summary>
        [Column("userGroupDefaultPermissions")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Obsolete("Is not used anymore. Use UserGroup2PermissionDtos instead. Scheduled for removal in Umbraco 18.")]
        public string? DefaultPermissions { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the user group.
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

        /// <summary>Gets or sets the icon associated with the user group.</summary>
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
        /// Gets or sets the start content ID associated with the user group.
        /// </summary>
        [Column("startContentId")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(NodeDto), Name = "FK_startContentId_umbracoNode_id")]
        public int? StartContentId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the media node where the user group starts in the media section.
        /// </summary>
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
        /// Gets or sets the collection of permissions associated with this user group.
        /// </summary>
        [ResultColumn]
        [Reference(ReferenceType.Many, ReferenceMemberName = "UserGroupId")]
        public List<UserGroup2PermissionDto> UserGroup2PermissionDtos { get; set; }

        /// <summary>
        /// Gets or sets the list of granular permissions associated with the user group.
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
