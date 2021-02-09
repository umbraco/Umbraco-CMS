using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to configure <see cref="BackOfficeIdentityOptions"/> for the Umbraco Back office
    /// </summary>
    public class ConfigureBackOfficeIdentityOptions : IConfigureOptions<BackOfficeIdentityOptions>
    {
        private readonly UserPasswordConfigurationSettings _userPasswordConfiguration;

        public ConfigureBackOfficeIdentityOptions(IOptions<UserPasswordConfigurationSettings> userPasswordConfiguration)
        {
            _userPasswordConfiguration = userPasswordConfiguration.Value;
        }

        public void Configure(BackOfficeIdentityOptions options)
        {
            options.User.RequireUniqueEmail = true;
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.SecurityStampClaimType = Constants.Security.SecurityStampClaimType;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            ConfigurePasswordOptions(_userPasswordConfiguration, options.Password);

            options.Lockout.MaxFailedAccessAttempts = _userPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
        }

        public static void ConfigurePasswordOptions(IPasswordConfiguration input, PasswordOptions output)
        {
            output.RequiredLength = input.RequiredLength;
            output.RequireNonAlphanumeric = input.RequireNonLetterOrDigit;
            output.RequireDigit = input.RequireDigit;
            output.RequireLowercase = input.RequireLowercase;
            output.RequireUppercase = input.RequireUppercase;
        }
    }
}
