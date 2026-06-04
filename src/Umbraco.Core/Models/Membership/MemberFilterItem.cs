// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents a member in a filtered listing, covering both content-based and external-only members.
/// </summary>
public class MemberFilterItem
{
    /// <summary>
    ///     Gets or sets the unique key of the member.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    ///     Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the member is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the member is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    ///     Gets or sets the last login date.
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    ///     Gets or sets the last lockout date.
    /// </summary>
    public DateTime? LastLockoutDate { get; set; }

    /// <summary>
    ///     Gets or sets the last password change date.
    /// </summary>
    public DateTime? LastPasswordChangeDate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this is an external-only member.
    /// </summary>
    public bool IsExternalOnly { get; set; }

    /// <summary>
    ///     Gets or sets the member type key. Null for external-only members.
    /// </summary>
    public Guid? MemberTypeKey { get; set; }

    /// <summary>
    ///     Gets or sets the member type name. Null for external-only members.
    /// </summary>
    public string? MemberTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the member type icon. Null for external-only members.
    /// </summary>
    public string? MemberTypeIcon { get; set; }

    /// <summary>
    ///     Gets or sets the member kind.
    /// </summary>
    public MemberKind Kind { get; set; }
}
