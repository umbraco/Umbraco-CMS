namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents the response model containing security configuration settings returned by the Umbraco CMS Management API.
/// </summary>
public class SecurityConfigurationResponseModel
{
    /// <summary>
    /// Gets or sets the configuration settings related to password policies.
    /// </summary>
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
