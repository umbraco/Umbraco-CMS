using System.Security.Claims;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Used to configure <see cref="BackOfficeIdentityOptions" /> for the Umbraco Back office
/// </summary>
public sealed class ConfigureBackOfficeIdentityOptions : IConfigureOptions<BackOfficeIdentityOptions>
{
    private readonly UserPasswordConfigurationSettings _userPasswordConfiguration;
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureBackOfficeIdentityOptions"/> class with the specified user password configuration and security settings.
    /// </summary>
    /// <param name="userPasswordConfiguration">The options containing user password configuration settings.</param>
    /// <param name="securitySettings">The options containing security settings.</param>
    public ConfigureBackOfficeIdentityOptions(
        IOptions<UserPasswordConfigurationSettings> userPasswordConfiguration,
        IOptions<SecuritySettings> securitySettings)
    {
        _userPasswordConfiguration = userPasswordConfiguration.Value;
        _securitySettings = securitySettings.Value;
    }

    /// <summary>
    /// Configures the specified <see cref="BackOfficeIdentityOptions"/> with security-related settings, including sign-in requirements, user validation, claims identity configuration, lockout policies, and password options for the Umbraco backoffice.
    /// </summary>
    /// <param name="options">The <see cref="BackOfficeIdentityOptions"/> instance to configure.</param>
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
