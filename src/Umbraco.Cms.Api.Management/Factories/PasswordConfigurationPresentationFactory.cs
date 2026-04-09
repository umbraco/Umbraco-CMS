using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordConfigurationPresentationFactory"/> class.
    /// </summary>
    /// <param name="userPasswordConfigurationSettings">An <see cref="IOptionsSnapshot{T}"/> containing the current <see cref="UserPasswordConfigurationSettings"/> for user password configuration.</param>
    [Obsolete("Use the constructor that accepts IOptionsSnapshot<SecuritySettings> instead. Scheduled for removal in Umbraco 19.")]
    public PasswordConfigurationPresentationFactory(IOptionsSnapshot<UserPasswordConfigurationSettings> userPasswordConfigurationSettings)
        : this(StaticServiceProvider.Instance.GetRequiredService<IOptionsSnapshot<SecuritySettings>>())
    {
    }

    [Obsolete("Use the constructor that accepts IOptionsSnapshot<SecuritySettings> instead. Scheduled for removal in Umbraco 19.")]
    public PasswordConfigurationPresentationFactory(IOptionsSnapshot<SecuritySettings> securitySettings, IOptionsSnapshot<UserPasswordConfigurationSettings> _)
        : this(securitySettings)
    {
    }

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
