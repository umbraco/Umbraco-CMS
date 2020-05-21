using System;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Used to configure <see cref="BackOfficeIdentityOptions"/> for the Umbraco Back office
    /// </summary>
    public class ConfigureUmbracoBackOfficeIdentityOptions : IConfigureOptions<BackOfficeIdentityOptions>
    {
        private readonly IUserPasswordConfiguration _userPasswordConfiguration;

        public ConfigureUmbracoBackOfficeIdentityOptions(IUserPasswordConfiguration userPasswordConfiguration)
        {
            _userPasswordConfiguration = userPasswordConfiguration;
        }

        public void Configure(BackOfficeIdentityOptions options)
        {
            options.User.RequireUniqueEmail = true;
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.SecurityStampClaimType = Constants.Web.SecurityStampClaimType;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            options.Password.RequiredLength = _userPasswordConfiguration.RequiredLength;
            options.Password.RequireNonAlphanumeric = _userPasswordConfiguration.RequireNonLetterOrDigit;
            options.Password.RequireDigit = _userPasswordConfiguration.RequireDigit;
            options.Password.RequireLowercase = _userPasswordConfiguration.RequireLowercase;
            options.Password.RequireUppercase = _userPasswordConfiguration.RequireUppercase;
            options.Lockout.MaxFailedAccessAttempts = _userPasswordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
        }
    }
}
