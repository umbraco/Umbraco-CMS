namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model for inviting a new user.
/// </summary>
public class InviteUserRequestModel : CreateUserRequestModelBase
{
    /// <summary>Optional message to include with the user invitation.</summary>
    public string? Message { get; set; }
}
