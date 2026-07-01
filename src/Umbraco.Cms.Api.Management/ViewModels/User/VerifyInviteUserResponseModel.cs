using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the response returned when verifying an invited user in the system.
/// </summary>
public class VerifyInviteUserResponseModel
{
    /// <summary>
    /// Gets or sets the password configuration settings applicable to the invited user.
    /// </summary>
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
