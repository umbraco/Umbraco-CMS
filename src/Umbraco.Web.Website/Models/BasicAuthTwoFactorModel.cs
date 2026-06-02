namespace Umbraco.Cms.Web.Website.Models;

/// <summary>
/// View model for the standalone basic auth two-factor authentication page.
/// </summary>
public class BasicAuthTwoFactorModel
{
    /// <summary>
    /// Gets or sets the local URL to redirect to after successful 2FA verification.
    /// </summary>
    public string? ReturnPath { get; set; }

    /// <summary>
    /// Gets or sets an error message to display on the 2FA form.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the enabled 2FA provider names for the user.
    /// </summary>
    public IEnumerable<string> ProviderNames { get; set; } = [];
}
