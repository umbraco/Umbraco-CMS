namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents the data required to change the password for the current user.
/// </summary>
public class ChangePasswordCurrentUserRequestModel : ChangePasswordUserRequestModel
{
    /// <summary>
    /// The old password.
    /// </summary>
    public string? OldPassword { get; set; }
}
