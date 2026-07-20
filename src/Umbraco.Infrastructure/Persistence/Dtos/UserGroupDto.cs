using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a user group in the Umbraco CMS infrastructure.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.UserGroup;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupDto"/> class with default values.
    /// </summary>
    public UserGroupDto()
    {
        UserGroup2AppDtos = new List<UserGroup2AppDto>();
        UserGroup2LanguageDtos = new List<UserGroup2LanguageDto>();
        UserGroup2PermissionDtos = new List<UserGroup2PermissionDto>();
        UserGroup2GranularPermissionDtos = new List<UserGroup2GranularPermissionDto>();
    }

    /// <summary>
    /// Gets or sets the unique identifier for the user group.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(IdentitySeed = 6)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier key for the user group.
    /// </summary>
    [Column(KeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUserGroup_userGroupKey")]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the alias of the user group.
    /// </summary>
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
    /// Gets or sets a textual description providing additional information about the user group.
    /// </summary>
    [Column(Name = "description")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the default permissions assigned to the user group.
    /// </summary>
    /// <remarks>
    /// This property is obsolete and no longer used. Use <c>UserGroup2PermissionDtos</c> instead.
    /// Scheduled for removal in Umbraco 18.
    /// </remarks>
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
    /// Gets or sets the icon associated with the user group.
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
    /// Gets or sets the ID of the root content node that members of the user group start at in the content tree.
    /// A null value indicates no specific start node is set.
    /// </summary>
    [Column("startContentId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto), Name = "FK_startContentId_umbracoNode_id")]
    public int? StartContentId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the media item that defines the starting point for media access for the user group.
    /// </summary>
    [Column("startMediaId")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [ForeignKey(typeof(NodeDto), Name = "FK_startMediaId_umbracoNode_id")]
    public int? StartMediaId { get; set; }

    /// <summary>
    /// Gets or sets the collection of application associations for this user group.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(UserGroup2AppDto.UserGroupId))]
    public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="UserGroup2LanguageDto"/> entities that define the languages associated with this user group.
    /// This property represents the many-to-many relationship between user groups and languages.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(UserGroup2LanguageDto.UserGroupId))]
    public List<UserGroup2LanguageDto> UserGroup2LanguageDtos { get; set; }

    /// <summary>
    /// Gets or sets the collection of permission mappings associated with this user group.
    /// Each item represents a link between the user group and a specific permission.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ColumnName = nameof(Key), ReferenceMemberName = nameof(UserGroup2PermissionDto.UserGroupKey))]
    public List<UserGroup2PermissionDto> UserGroup2PermissionDtos { get; set; }

    /// <summary>
    /// Gets or sets the list of granular permissions associated with the user group.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ColumnName = nameof(Key), ReferenceMemberName = nameof(UserGroup2GranularPermissionDto.UserGroupKey))]
    public List<UserGroup2GranularPermissionDto> UserGroup2GranularPermissionDtos { get; set; }

    /// <summary>
    ///     This is only relevant when this column is included in the results (i.e. GetUserGroupsWithUserCounts).
    /// </summary>
    [ResultColumn]
    public int UserCount { get; set; }
}
