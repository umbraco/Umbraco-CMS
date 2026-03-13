using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

/// <summary>
/// Represents a member returned in a response from the Umbraco CMS Management API.
/// </summary>
public class MemberResponseModel : ContentResponseModelBase<MemberValueResponseModel, MemberVariantResponseModel>
{
    /// <summary>
    /// Gets or sets the email address of the member.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the member.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the member type reference information.
    /// </summary>
    public MemberTypeReferenceResponseModel MemberType { get; set; } = new();

    /// <summary>Gets or sets a value indicating whether the member is approved.</summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the member is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled for the member.
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; }

    /// <summary>Gets or sets the number of failed password attempts for the member.</summary>
    public int FailedPasswordAttempts { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the member's last login.
    /// </summary>
    public DateTimeOffset? LastLoginDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the member was last locked out.
    /// </summary>
    public DateTimeOffset? LastLockoutDate { get; set; }

    /// <summary>Gets or sets the date and time when the member last changed their password.</summary>
    public DateTimeOffset? LastPasswordChangeDate { get; set; }

    /// <summary>
    /// Gets or sets the collection of group IDs associated with the member.
    /// </summary>
    public IEnumerable<Guid> Groups { get; set; } = [];

    /// <summary>
    /// Gets or sets the classification of the member, indicating the type or category of the member entity.
    /// </summary>
    public MemberKind Kind { get; set; }
}
