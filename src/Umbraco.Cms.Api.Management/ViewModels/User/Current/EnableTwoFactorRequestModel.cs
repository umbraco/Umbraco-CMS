namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

/// <summary>
/// Represents a request model to enable two-factor authentication for the current user.
/// </summary>
public class EnableTwoFactorRequestModel
{
    /// <summary>
    /// Gets or sets the verification code used to enable two-factor authentication.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Gets or sets the secret key used to enable two-factor authentication.
    /// </summary>
    public required string Secret { get; set; }

}
