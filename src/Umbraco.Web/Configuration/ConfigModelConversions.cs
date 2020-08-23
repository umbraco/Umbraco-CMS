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
    /// converting between <see cref="IGlobalSettings"></see> (used by
    /// legacy configuration and <see cref="GlobalSettings"></see> (used by Netcore/IOptions configuration).
    /// </summary>
    public static class ConfigModelConversions
    {
        public static GlobalSettings ConvertGlobalSettings(IGlobalSettings globalSettings)
        {
            return new GlobalSettings
            {
                DatabaseFactoryServerVersion = globalSettings.DatabaseFactoryServerVersion,
                DefaultUILanguage = globalSettings.DefaultUILanguage,
                DisableElectionForSingleServer = globalSettings.DisableElectionForSingleServer,
                HideTopLevelNodeFromPath = globalSettings.HideTopLevelNodeFromPath,
                InstallEmptyDatabase = globalSettings.InstallEmptyDatabase,
                InstallMissingDatabase = globalSettings.InstallMissingDatabase,
                MainDomLock = globalSettings.MainDomLock,
                NoNodesViewPath = globalSettings.NoNodesViewPath,
                RegisterType = globalSettings.RegisterType,
                ReservedPaths = globalSettings.ReservedPaths,
                ReservedUrls = globalSettings.ReservedUrls,
                SmtpSettings = new SmtpSettings
                {
                    DeliveryMethod = globalSettings.SmtpSettings.DeliveryMethod,
                    From = globalSettings.SmtpSettings.From,
                    Host = globalSettings.SmtpSettings.Host,
                    Password = globalSettings.SmtpSettings.Password,
                    PickupDirectoryLocation = globalSettings.SmtpSettings.PickupDirectoryLocation,
                    Port = globalSettings.SmtpSettings.Port,
                    Username = globalSettings.SmtpSettings.Username,
                },
                TimeOutInMinutes = globalSettings.TimeOutInMinutes,
                UmbracoCssPath = globalSettings.UmbracoCssPath,
                UmbracoMediaPath = globalSettings.UmbracoMediaPath,
                UmbracoPath = globalSettings.UmbracoPath,
                UmbracoScriptsPath = globalSettings.UmbracoScriptsPath,
                UseHttps = globalSettings.UseHttps,
                VersionCheckPeriod = globalSettings.VersionCheckPeriod,
            };
        }

        public static Umbraco.Core.Configuration.Models.ConnectionStrings ConvertConnectionStrings(IConnectionStrings connectionStrings)
        {
            return new Umbraco.Core.Configuration.Models.ConnectionStrings
            {
                UmbracoConnectionString = connectionStrings[Constants.System.UmbracoConnectionName].ConnectionString
            };
        }

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
