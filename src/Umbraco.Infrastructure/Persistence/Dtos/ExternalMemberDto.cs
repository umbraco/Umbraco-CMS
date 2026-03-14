// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[ExplicitColumns]
[PrimaryKey(PrimaryKeyColumnName)]
internal sealed class ExternalMemberDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.ExternalMember;
    public const string PrimaryKeyColumnName = "id";

    /// <summary>
    /// Gets or sets the unique identifier for the external member.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique key for the external member.
    /// </summary>
    [Column("key")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_Key")]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the email address of the external member.
    /// </summary>
    [Column("email")]
    [Length(1000)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the username of the external member.
    /// </summary>
    [Column("userName")]
    [Length(1000)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_UserName")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display name of the external member.
    /// </summary>
    [Column("name")]
    [Length(1000)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the external member is approved.
    /// </summary>
    [Column("isApproved")]
    [Constraint(Default = "1")]
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the external member is locked out.
    /// </summary>
    [Column("isLockedOut")]
    [Constraint(Default = "0")]
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the last login.
    /// </summary>
    [Column("lastLoginDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the last lockout.
    /// </summary>
    [Column("lastLockoutDate")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the external member was created.
    /// </summary>
    [Column("createDate")]
    [Constraint(Default = SystemMethods.CurrentUTCDateTime)]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Gets or sets the security stamp for the external member.
    /// </summary>
    [Column("securityStamp")]
    [Length(255)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the serialized profile data for the external member.
    /// </summary>
    [Column("profileData")]
    [NullSetting(NullSetting = NullSettings.Null)]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    public string? ProfileData { get; set; }
}
