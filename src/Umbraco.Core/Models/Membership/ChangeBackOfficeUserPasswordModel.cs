namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the model for changing a backoffice user's password.
/// </summary>
public class ChangeBackOfficeUserPasswordModel
{
    /// <summary>
    ///     Gets or sets the new password to set for the user.
    /// </summary>
    public required string NewPassword { get; set; }

    /// <summary>
    ///     The old password - used to change a password when: EnablePasswordRetrieval = false
    /// </summary>
    public string? OldPassword { get; set; }

    /// <summary>
    ///     The user requesting the password change
    /// </summary>
    public required IUser User { get; set; }

    /// <summary>
    ///     The reset token that is required if changing your own password without the old password.
    /// </summary>
    public string? ResetPasswordToken { get; set; }
}
