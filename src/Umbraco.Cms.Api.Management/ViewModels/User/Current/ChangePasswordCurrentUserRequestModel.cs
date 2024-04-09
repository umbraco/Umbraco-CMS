namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class ChangePasswordCurrentUserRequestModel : ChangePasswordUserRequestModel
{
    /// <summary>
    /// The old password.
    /// </summary>
    public string? OldPassword { get; set; }
}
