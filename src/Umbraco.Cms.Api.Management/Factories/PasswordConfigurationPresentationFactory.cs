using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for password configuration in the management API.
/// </summary>
public class PasswordConfigurationPresentationFactory : IPasswordConfigurationPresentationFactory
{
    private readonly UserPasswordConfigurationSettings _userPasswordConfigurationSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="userPasswordConfigurationSettings">An <see cref="IOptionsSnapshot{T}"/> containing the current <see cref="UserPasswordConfigurationSettings"/> for user password configuration.</param>
    public PasswordConfigurationPresentationFactory(IOptionsSnapshot<UserPasswordConfigurationSettings> userPasswordConfigurationSettings) => _userPasswordConfigurationSettings = userPasswordConfigurationSettings.Value;

    public PasswordConfigurationResponseModel CreatePasswordConfigurationResponseModel() =>
        new()
        {
            MinimumPasswordLength = _userPasswordConfigurationSettings.RequiredLength,
            RequireNonLetterOrDigit = _userPasswordConfigurationSettings.RequireNonLetterOrDigit,
            RequireDigit = _userPasswordConfigurationSettings.RequireDigit,
            RequireLowercase = _userPasswordConfigurationSettings.RequireLowercase,
            RequireUppercase = _userPasswordConfigurationSettings.RequireUppercase,
        };
}
