namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Represents the model for changing a user's password.
/// </summary>
public class ChangeUserPasswordModel
{
    /// <summary>
    ///     Gets or sets the new password to set for the user.
    /// </summary>
    public required string NewPassword { get; set; }

    /// <summary>
    ///     Gets or sets the old password for validation when changing a password.
    /// </summary>
    public string? OldPassword { get; set; }

    /// <summary>
    ///     Gets or sets the reset password token used for password reset operations.
    /// </summary>
    public string? ResetPasswordToken { get; set; }

    /// <summary>
    ///     Gets or sets the unique key identifying the user whose password is being changed.
    /// </summary>
    public Guid UserKey { get; set; }
}
