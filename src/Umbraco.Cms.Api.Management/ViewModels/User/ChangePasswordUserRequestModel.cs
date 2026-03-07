namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the data required to request a password change for a user.
/// </summary>
public class ChangePasswordUserRequestModel
{
    /// <summary>
    /// The new password.
    /// </summary>
    public required string NewPassword { get; set; }
}
