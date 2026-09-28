using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for password configuration in the management API.
/// </summary>
public class PasswordConfigurationPresentationFactory : IPasswordConfigurationPresentationFactory
{
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="securitySettings">An <see cref="IOptionsSnapshot{T}"/> containing the current <see cref="SecuritySettings"/> for user password configuration.</param>
    public PasswordConfigurationPresentationFactory(IOptionsSnapshot<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    public PasswordConfigurationResponseModel CreatePasswordConfigurationResponseModel() =>
        new()
        {
            MinimumPasswordLength = _securitySettings.UserPassword.RequiredLength,
            RequireNonLetterOrDigit = _securitySettings.UserPassword.RequireNonLetterOrDigit,
            RequireDigit = _securitySettings.UserPassword.RequireDigit,
            RequireLowercase = _securitySettings.UserPassword.RequireLowercase,
            RequireUppercase = _securitySettings.UserPassword.RequireUppercase,
        };
}
