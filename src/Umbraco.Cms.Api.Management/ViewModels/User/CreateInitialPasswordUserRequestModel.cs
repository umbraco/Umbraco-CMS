namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model used to create a user with an initial password in the system.
/// </summary>
public class CreateInitialPasswordUserRequestModel : VerifyInviteUserRequestModel
{
    /// <summary>Gets or sets the initial password for the user.</summary>
    public string Password { get; set; } = string.Empty;
}
