using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.Configuration
{
    /// <summary>
    /// TEMPORARY: this class has been added just to ensure Umbraco.Web functionality continues to compile, by
    /// converting between e.g. <see cref="BackOfficeIdentityOptions"></see> (used by
    /// legacy configuration and <see cref="UserPasswordConfigurationSettings"></see> (used by Netcore/IOptions configuration).
    /// </summary>
    public static class ConfigModelConversions
    {
        public static IOptions<UserPasswordConfigurationSettings> ConvertToOptionsOfUserPasswordConfigurationSettings(IOptions<BackOfficeIdentityOptions> identityOptions)
        {
            var passwordOptions = identityOptions.Value.Password;
            var lockOutOptions = identityOptions.Value.Lockout;
            var passwordConfiguration = new UserPasswordConfigurationSettings
            {
                MaxFailedAccessAttemptsBeforeLockout = lockOutOptions.MaxFailedAccessAttempts,
                HashAlgorithmType = Constants.Security.AspNetCoreV3PasswordHashAlgorithmName,  // TODO: not sure where to map this from.
                RequireDigit = passwordOptions.RequireDigit,
                RequiredLength = passwordOptions.RequiredLength,
                RequireLowercase = passwordOptions.RequireLowercase,
                RequireNonLetterOrDigit = passwordOptions.RequireNonAlphanumeric,
                RequireUppercase = passwordOptions.RequireUppercase,
            };

            return Options.Create(passwordConfiguration);
        }

        public static IOptions<BackOfficeIdentityOptions> ConvertToOptionsOfBackOfficeIdentityOptions(IUserPasswordConfiguration passwordConfiguration)
        {
            var identityOptions = new BackOfficeIdentityOptions
            {
                Lockout = new LockoutOptions
                {
                    MaxFailedAccessAttempts = passwordConfiguration.MaxFailedAccessAttemptsBeforeLockout,
                },
                Password = new PasswordOptions
                {
                    RequireDigit = passwordConfiguration.RequireDigit,
                    RequiredLength = passwordConfiguration.RequiredLength,
                    RequireLowercase = passwordConfiguration.RequireLowercase,
                    RequireNonAlphanumeric = passwordConfiguration.RequireNonLetterOrDigit,
                    RequireUppercase = passwordConfiguration.RequireUppercase,
                }
            };

            return Options.Create(identityOptions);
        }
    }
}
