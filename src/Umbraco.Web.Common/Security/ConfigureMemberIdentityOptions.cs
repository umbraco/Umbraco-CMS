using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public sealed class ConfigureMemberIdentityOptions : IConfigureOptions<IdentityOptions>
{
    private readonly SecuritySettings _securitySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigureMemberIdentityOptions" /> class.
    /// </summary>
    /// <param name="securitySettings">The security configuration.</param>
    public ConfigureMemberIdentityOptions(
        IOptions<SecuritySettings> securitySettings)
    {
        _securitySettings = securitySettings.Value;
    }

    [Obsolete("Use the constructor that only takes IOptions<SecuritySettings> instead. Scheduled for removal in Umbraco 19.")]
    public ConfigureMemberIdentityOptions(
        IOptions<MemberPasswordConfigurationSettings> memberPasswordConfiguration,
        IOptions<SecuritySettings> securitySettings)
        : this(securitySettings)
    {
    }

    public void Configure(IdentityOptions options)
    {
        options.SignIn.RequireConfirmedAccount = true; // uses our custom IUserConfirmation
        options.SignIn.RequireConfirmedEmail = false; // not implemented
        options.SignIn.RequireConfirmedPhoneNumber = false; // not implemented

        options.User.RequireUniqueEmail = _securitySettings.MemberRequireUniqueEmail;

        // Support validation of member names using Down-Level Logon Name format
        options.User.AllowedUserNameCharacters = _securitySettings.AllowedUserNameCharacters;

        options.Lockout.AllowedForNewUsers = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(_securitySettings.MemberDefaultLockoutTimeInMinutes);

        options.Password.ConfigurePasswordOptions(_securitySettings.MemberPassword);

        options.Lockout.MaxFailedAccessAttempts = _securitySettings.MemberPassword.MaxFailedAccessAttemptsBeforeLockout;
    }
}
