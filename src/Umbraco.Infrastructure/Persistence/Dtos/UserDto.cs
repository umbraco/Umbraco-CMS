using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
public class UserDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User;

    public UserDto()
    {
        UserGroupDtos = new List<UserGroupDto>();
        UserStartNodeDtos = new HashSet<UserStartNodeDto>();
    }

    [Column("id")]
    [PrimaryKeyColumn(Name = "PK_user")]
    public int Id { get; set; }

    [Column("userDisabled")]
    [Constraint(Default = "0")]
    public bool Disabled { get; set; }

    [Column("key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUser_userKey")]
    public Guid Key { get; set; }

    [Column("userNoConsole")]
    [Constraint(Default = "0")]
    public bool NoConsole { get; set; }

    [Column("userName")]
    public string UserName { get; set; } = null!;

    [Column("userLogin")]
    [Length(125)]
    [Index(IndexTypes.NonClustered)]
    public string? Login { get; set; }

    [Column("userPassword")]
    [Length(500)]
    public string? Password { get; set; }

    /// <summary>
    ///     This will represent a JSON structure of how the password has been created (i.e hash algorithm, iterations)
    /// </summary>
    [Column("passwordConfig")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? PasswordConfig { get; set; }

    [Column("userEmail")]
    public string Email { get; set; } = null!;

    [Column("userLanguage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(10)]
    public string? UserLanguage { get; set; }

    [Column("securityStampToken")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? SecurityStampToken { get; set; }

    [Column("failedLoginAttempts")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? FailedLoginAttempts { get; set; }

    [Column("lastLockoutDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLockoutDate { get; set; }

    [Column("lastPasswordChangeDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastPasswordChangeDate { get; set; }

    [Column("lastLoginDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLoginDate { get; set; }

    [Column("emailConfirmedDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? EmailConfirmedDate { get; set; }

    [Column("invitedDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? InvitedDate { get; set; }

    [Column("createDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime CreateDate { get; set; } = DateTime.Now;

    [Column("updateDate", ForceToUtc = false)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentDateTime)]
    public DateTime UpdateDate { get; set; } = DateTime.Now;

    [Column("kind")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = 0)]
    public short Kind { get; set; }

    /// <summary>
    ///     Will hold the media file system relative path of the users custom avatar if they uploaded one
    /// </summary>
    [Column("avatar")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? Avatar { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
    public List<UserGroupDto> UserGroupDtos { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "UserId")]
    public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
}
