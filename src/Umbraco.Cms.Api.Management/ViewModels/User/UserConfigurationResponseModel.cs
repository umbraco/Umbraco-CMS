using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a response model containing configuration settings specific to a user.
/// </summary>
public class UserConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the user can invite other users.
    /// </summary>
    public bool CanInviteUsers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the username is in the form of an email address.
    /// </summary>
    public bool UsernameIsEmail { get; set; }

    /// <summary>
    /// Gets or sets the configuration settings related to the user's password policy.
    /// </summary>
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is allowed to change their password.
    /// </summary>
    public bool AllowChangePassword { get; set; }

    /// <summary>Gets or sets a value indicating whether two-factor authentication is allowed for the user.</summary>
    public bool AllowTwoFactor { get; set; }
}
