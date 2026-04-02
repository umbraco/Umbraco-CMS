namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents a model for updating a member.
/// </summary>
public class MemberUpdateModel : MemberEditingModelBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the member is locked out.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether two-factor authentication is enabled for the member.
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the old password for password change verification.
    /// </summary>
    public string? OldPassword { get; set; }

    /// <summary>
    ///     Gets or sets the new password to set for the member.
    /// </summary>
    public string? NewPassword { get; set; }
}
