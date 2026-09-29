using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents the response model containing configuration data for the currently authenticated user.
/// </summary>
public class CurrentUserConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the user should be kept logged in.
    /// </summary>
    public bool KeepUserLoggedIn { get; set; }

    /// <summary>
    /// Gets or sets the absolute UTC time at which the current session expires,
    /// or <c>null</c> when the session has no fixed expiry (e.g. <see cref="KeepUserLoggedIn"/> is set).
    /// </summary>
    public DateTimeOffset? TimeoutUtc { get; set; }

    /// <summary>
    /// Gets or sets the password configuration for the current user.
    /// </summary>
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current user is allowed to change their password.
    /// </summary>
    public bool AllowChangePassword { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether two-factor authentication is allowed for the current user.
    /// </summary>
    public bool AllowTwoFactor { get; set; }
}
