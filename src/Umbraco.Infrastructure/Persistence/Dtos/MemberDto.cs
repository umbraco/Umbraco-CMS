using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = false)]
[ExplicitColumns]
internal sealed class MemberDto
{
    private const string TableName = Constants.DatabaseSchema.Tables.Member;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.NodeIdName;

    /// <summary>
    /// Gets or sets the node identifier associated with the member.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the member.
    /// </summary>
    [Column("Email")]
    [Length(1000)]
    [Constraint(Default = "''")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the login name associated with the member account.
    /// </summary>
    [Column("LoginName")]
    [Length(1000)]
    [Constraint(Default = "''")]
    [Index(IndexTypes.NonClustered, Name = "IX_cmsMember_LoginName")]
    public string LoginName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the hashed password of the member.
    /// This value is typically stored securely and should not be the plain text password.
    /// </summary>
    [Column("Password")]
    [Length(1000)]
    [Constraint(Default = "''")]
    public string? Password { get; set; }

    /// <summary>
    ///     This will represent a JSON structure of how the password has been created (i.e hash algorithm, iterations)
    /// </summary>
    [Column("passwordConfig")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(500)]
    public string? PasswordConfig { get; set; }

    /// <summary>
    /// Gets or sets the security stamp token associated with the member, used for validating security-related operations such as password changes or authentication.
    /// </summary>
    [Column("securityStampToken")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? SecurityStampToken { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member's email was confirmed.
    /// </summary>
    [Column("emailConfirmedDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? EmailConfirmedDate { get; set; }

    /// <summary>
    /// Gets or sets the number of failed password attempts.
    /// </summary>
    [Column("failedPasswordAttempts")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? FailedPasswordAttempts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is locked out.
    /// </summary>
    [Column("isLockedOut")]
    [Constraint(Default = 0)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is approved.
    /// </summary>
    [Column("isApproved")]
    [Constraint(Default = 1)]
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member last logged in.
    /// </summary>
    [Column("lastLoginDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member was last locked out.
    /// </summary>
    [Column("lastLockoutDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member last changed their password.
    /// </summary>
    [Column("lastPasswordChangeDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastPasswordChangeDate { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ContentDto"/> entity that represents the content data associated with this member.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentDto.NodeId))]
    public ContentDto ContentDto { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content version DTO related to this member.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = nameof(ContentVersionDto.NodeId))]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
