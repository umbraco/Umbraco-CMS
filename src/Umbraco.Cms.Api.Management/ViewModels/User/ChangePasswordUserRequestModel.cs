namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class ChangePasswordUserRequestModel
{
    /// <summary>
    /// The new password.
    /// </summary>
    public required string NewPassword { get; set; }

    /// <summary>
    /// The old password.
    /// </summary>
    public string? OldPassword { get; set; }
}
