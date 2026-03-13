using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Member;

/// <summary>
/// Represents the model used to update a member's information.
/// </summary>
public class UpdateMemberRequestModel : UpdateContentRequestModelBase<MemberValueModel, MemberVariantRequestModel>
{
    /// <summary>
    /// Gets or sets the email address of the member.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username associated with the member account.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous password of the member, used for validating password changes.
    /// </summary>
    public string? OldPassword { get; set; }

    /// <summary>
    /// Gets or sets the new password to assign to the member during an update operation.
    /// </summary>
    public string? NewPassword { get; set; }

    /// <summary>
    /// Gets or sets the collection of group IDs associated with the member.
    /// </summary>
    public IEnumerable<Guid>? Groups { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the member is approved.
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>Gets or sets a value indicating whether the member is locked out.</summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is enabled for the member.
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; }
}
