namespace Umbraco.Cms.Core.Models.Membership;

public class ChangeBackOfficeUserPasswordModel
{
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
