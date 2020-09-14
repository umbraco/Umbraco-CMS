using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Infrastructure.Configuration
{
    /// <summary>
    /// TEMPORARY: this class has been added just to ensure Umbraco.Web functionality continues to compile, by
    /// converting between e.g. <see cref="IGlobalSettings"></see> (used by
    /// legacy configuration and <see cref="GlobalSettings"></see> (used by Netcore/IOptions configuration).
    /// </summary>
    public static class ConfigModelConversionsFromLegacy
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
                Smtp = globalSettings.SmtpSettings != null
                    ? new SmtpSettings
                    {
                        DeliveryMethod = globalSettings.SmtpSettings.DeliveryMethod,
                        From = globalSettings.SmtpSettings.From,
                        Host = globalSettings.SmtpSettings.Host,
                        Password = globalSettings.SmtpSettings.Password,
                        PickupDirectoryLocation = globalSettings.SmtpSettings.PickupDirectoryLocation,
                        Port = globalSettings.SmtpSettings.Port,
                        Username = globalSettings.SmtpSettings.Username,
                    }
                    : new SmtpSettings(),
                TimeOutInMinutes = globalSettings.TimeOutInMinutes,
                UmbracoCssPath = globalSettings.UmbracoCssPath,
                UmbracoMediaPath = globalSettings.UmbracoMediaPath,
                UmbracoPath = globalSettings.UmbracoPath,
                UmbracoScriptsPath = globalSettings.UmbracoScriptsPath,
                IconsPath = globalSettings.IconsPath,
                UseHttps = globalSettings.UseHttps,
                VersionCheckPeriod = globalSettings.VersionCheckPeriod,
            };
        }

        public static UserPasswordConfigurationSettings ConvertUserPasswordConfiguration(IUserPasswordConfiguration passwordConfiguration)
        {
            return new UserPasswordConfigurationSettings
            {
                HashAlgorithmType = passwordConfiguration.HashAlgorithmType,
                RequireDigit = passwordConfiguration.RequireDigit,
                RequiredLength = passwordConfiguration.RequiredLength,
                RequireLowercase = passwordConfiguration.RequireLowercase,
                RequireNonLetterOrDigit = passwordConfiguration.RequireNonLetterOrDigit,
                RequireUppercase = passwordConfiguration.RequireUppercase,
            };
        }
    }
}
