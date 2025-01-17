using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public class PasswordConfigurationPresentationFactory : IPasswordConfigurationPresentationFactory
{
    private readonly UserPasswordConfigurationSettings _userPasswordConfigurationSettings;

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
