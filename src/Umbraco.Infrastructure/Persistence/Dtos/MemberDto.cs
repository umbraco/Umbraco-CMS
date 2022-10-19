using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("nodeId", AutoIncrement = false)]
[ExplicitColumns]
internal class MemberDto
{
    private const string TableName = Constants.DatabaseSchema.Tables.Member;

    [Column("nodeId")]
    [PrimaryKeyColumn(AutoIncrement = false)]
    [ForeignKey(typeof(ContentDto))]
    public int NodeId { get; set; }

    [Column("Email")]
    [Length(1000)]
    [Constraint(Default = "''")]
    public string Email { get; set; } = null!;

    [Column("LoginName")]
    [Length(1000)]
    [Constraint(Default = "''")]
    [Index(IndexTypes.NonClustered, Name = "IX_cmsMember_LoginName")]
    public string LoginName { get; set; } = null!;

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

    [Column("securityStampToken")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [Length(255)]
    public string? SecurityStampToken { get; set; }

    [Column("emailConfirmedDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? EmailConfirmedDate { get; set; }

    [Column("failedPasswordAttempts")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public int? FailedPasswordAttempts { get; set; }

    [Column("isLockedOut")]
    [Constraint(Default = 0)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public bool IsLockedOut { get; set; }

    [Column("isApproved")]
    [Constraint(Default = 1)]
    public bool IsApproved { get; set; }

    [Column("lastLoginDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLoginDate { get; set; }

    [Column("lastLockoutDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLockoutDate { get; set; }

    [Column("lastPasswordChangeDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastPasswordChangeDate { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
    public ContentDto ContentDto { get; set; } = null!;

    [ResultColumn]
    [Reference(ReferenceType.OneToOne, ReferenceMemberName = "NodeId")]
    public ContentVersionDto ContentVersionDto { get; set; } = null!;
}
