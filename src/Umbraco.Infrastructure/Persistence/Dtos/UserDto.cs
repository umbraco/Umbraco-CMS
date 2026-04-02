using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

/// <summary>
/// Data transfer object representing a user in the Umbraco CMS persistence layer.
/// </summary>
[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class UserDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;
    public const string KeyColumnName = "key";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDto"/> class with default values.
    /// </summary>
    public UserDto()
    {
        UserGroupDtos = new List<UserGroupDto>();
        UserStartNodeDtos = new HashSet<UserStartNodeDto>();
    }

    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(Name = "PK_user")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is disabled.
    /// </summary>
    [Column("userDisabled")]
    [Constraint(Default = "0")]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the unique GUID key that identifies the user.
    /// </summary>
    [Column(KeyColumnName)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.NewGuid)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoUser_userKey")]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is prevented from accessing the console.
    /// </summary>
    [Column("userNoConsole")]
    [Constraint(Default = "0")]
    public bool NoConsole { get; set; }

    /// <summary>
    /// Gets or sets the unique user name associated with the user.
    /// </summary>
    [Column("userName")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username used for logging in to the user account.
    /// </summary>
    [Column("userLogin")]
    [Length(125)]
    [Index(IndexTypes.NonClustered)]
    public string? Login { get; set; }

    /// <summary>
    /// Gets or sets the user's hashed password.
    /// This value should never contain the plain text password.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [Column("userEmail")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the ISO language code representing the user's preferred language.
    /// </summary>
    [Column("userLanguage")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(10)]
    public string? UserLanguage { get; set; }

    /// <summary>
    /// Gets or sets the security stamp token for the user, which is used to validate the user's security credentials and detect changes such as password updates.
    /// </summary>
    [Column("securityStampToken")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? SecurityStampToken { get; set; }

    /// <summary>
    /// Gets or sets the number of failed login attempts.
    /// </summary>
    [Column("failedLoginAttempts")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? FailedLoginAttempts { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was last locked out.
    /// </summary>
    [Column("lastLockoutDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user's password was last changed.
    /// </summary>
    [Column("lastPasswordChangeDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastPasswordChangeDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user last logged in.
    /// </summary>
    [Column("lastLoginDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user's email was confirmed.
    /// </summary>
    [Column("emailConfirmedDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? EmailConfirmedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the user was invited.
    /// </summary>
    [Column("invitedDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? InvitedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    [Column("createDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the user was last updated.
    /// </summary>
    [Column("updateDate")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the kind of the user, typically used to distinguish between different user types or roles within the system.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the user groups associated with this user.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(User2UserGroupDto.UserId))]
    public List<UserGroupDto> UserGroupDtos { get; set; }

    /// <summary>
    /// Gets or sets the collection of start node DTOs associated with the user.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(UserStartNodeDto.UserId))]
    public HashSet<UserStartNodeDto> UserStartNodeDtos { get; set; }
}
