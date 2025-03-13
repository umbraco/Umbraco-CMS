using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Used to configure <see cref="BackOfficeIdentityOptions" /> for the Umbraco Back office
/// </summary>
public sealed class ConfigureBackOfficeIdentityOptions : IConfigureOptions<BackOfficeIdentityOptions>
{
    private readonly UserPasswordConfigurationSettings _userPasswordConfiguration;
    private readonly SecuritySettings _securitySettings;

    [Obsolete("Use the constructor that accepts SecuritySettings. Will be removed in V13.")]
    public ConfigureBackOfficeIdentityOptions(IOptions<UserPasswordConfigurationSettings> userPasswordConfiguration)
        : this(userPasswordConfiguration, StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>())
    {
    }

    public ConfigureBackOfficeIdentityOptions(
        IOptions<UserPasswordConfigurationSettings> userPasswordConfiguration,
        IOptions<SecuritySettings> securitySettings)
    {
        _userPasswordConfiguration = userPasswordConfiguration.Value;
        _securitySettings = securitySettings.Value;
    }

    public void Configure(BackOfficeIdentityOptions options)
    {
        options.SignIn.RequireConfirmedAccount = true; // uses our custom IUserConfirmation
        options.SignIn.RequireConfirmedEmail = false; // not implemented
        options.SignIn.RequireConfirmedPhoneNumber = false; // not implemented

        options.User.RequireUniqueEmail = true;
        // Support validation of users names using Down-Level Logon Name format
        options.User.AllowedUserNameCharacters = _securitySettings.AllowedUserNameCharacters;

        options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
        options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
        options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
        options.ClaimsIdentity.SecurityStampClaimType = Constants.Security.SecurityStampClaimType;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(_securitySettings.UserDefaultLockoutTimeInMinutes);

        options.Password.ConfigurePasswordOptions(_userPasswordConfiguration);

        options.Lockout.MaxFailedAccessAttempts = _userPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
    }
}
