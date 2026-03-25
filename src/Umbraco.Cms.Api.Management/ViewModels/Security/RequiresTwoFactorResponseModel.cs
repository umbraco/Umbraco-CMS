namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents the response model indicating whether two-factor authentication is required.
/// </summary>
public class RequiresTwoFactorResponseModel
{
    /// <summary>
    /// Gets or sets the name or path of the view used for two-factor login.
    /// </summary>
    public string? TwoFactorLoginView { get; set; }
    /// <summary>
    /// Gets or sets the names of the enabled two-factor authentication providers.
    /// </summary>
    public required IEnumerable<string> EnabledTwoFactorProviderNames { get; set; }
}
