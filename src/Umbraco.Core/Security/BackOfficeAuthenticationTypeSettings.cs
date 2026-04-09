using Umbraco.Cms.Core;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Configuration settings for back office authentication types.
/// </summary>
public class BackOfficeAuthenticationTypeSettings
{
    /// <summary>
    ///     Gets or sets the authentication type for the back office.
    /// </summary>
    public string AuthenticationType { get; set; } = Constants.Security.BackOfficeAuthenticationType;

    /// <summary>
    ///     Gets or sets the authentication type for external back office authentication.
    /// </summary>
    public string ExternalAuthenticationType { get; set; } = Constants.Security.BackOfficeExternalAuthenticationType;

    /// <summary>
    ///     Gets or sets the authentication type for back office two-factor authentication.
    /// </summary>
    public string TwoFactorAuthenticationType { get; set; } = Constants.Security.BackOfficeTwoFactorAuthenticationType;

    /// <summary>
    ///     Gets or sets the authentication type for back office two-factor remember me authentication.
    /// </summary>
    public string TwoFactorRememberMeAuthenticationType { get; set; } = Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;
}
