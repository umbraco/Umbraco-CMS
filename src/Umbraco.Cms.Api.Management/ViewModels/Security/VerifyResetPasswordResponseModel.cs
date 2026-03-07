namespace Umbraco.Cms.Api.Management.ViewModels.Security;

    /// <summary>
    /// Represents the response returned after verifying a password reset request.
    /// </summary>
public class VerifyResetPasswordResponseModel
{
    /// <summary>
    /// Gets or sets the configuration details for password requirements.
    /// </summary>
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
