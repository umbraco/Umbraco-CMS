namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents the data model for a user login request in the security API.
/// </summary>
public class LoginRequestModel
{
    /// <summary>
    /// Gets or sets the username used to authenticate the user during login.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets or sets the password for the login request.
    /// </summary>
    public required string Password { get; init; }
}
