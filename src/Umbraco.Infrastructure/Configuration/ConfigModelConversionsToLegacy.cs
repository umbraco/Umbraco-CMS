using System.Collections.Generic;
using System.Net.Mail;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Infrastructure.Configuration
{
    /// <summary>
    /// TEMPORARY: this class has been added just to ensure tests on Umbraco.Web functionality, that still use the interface
    /// based configuration, by converting between e.g <see cref="GlobalSettings"></see> (used by
    /// legacy configuration and <see cref="IGlobalSettings"></see> (used by Netcore/IOptions configuration).
    /// </summary>
    public static class ConfigModelConversionsToLegacy
    {
        public static IGlobalSettings ConvertGlobalSettings(GlobalSettings globalSettings)
        {
            return new TestGlobalSettings
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
                SmtpSettings = new TestSmtpSettings
                {
                    DeliveryMethod = globalSettings.Smtp.DeliveryMethod,
                    From = globalSettings.Smtp.From,
                    Host = globalSettings.Smtp.Host,
                    Password = globalSettings.Smtp.Password,
                    PickupDirectoryLocation = globalSettings.Smtp.PickupDirectoryLocation,
                    Port = globalSettings.Smtp.Port,
                    Username = globalSettings.Smtp.Username,
                },
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

        public static IConnectionStrings ConvertConnectionStrings(ConnectionStrings connectionStrings)
        {
            var result = new TestConnectionStrings();
            result.AddEntry(Constants.System.UmbracoConnectionName, connectionStrings.UmbracoConnectionString);
            return result;
        }

        public static IUserPasswordConfiguration ConvertUserPasswordConfiguration(UserPasswordConfigurationSettings passwordConfiguration)
        {
            return new TestUserPasswordConfiguration
            {
                HashAlgorithmType = passwordConfiguration.HashAlgorithmType,
                RequireDigit = passwordConfiguration.RequireDigit,
                RequiredLength = passwordConfiguration.RequiredLength,
                RequireLowercase = passwordConfiguration.RequireLowercase,
                RequireNonLetterOrDigit = passwordConfiguration.RequireNonLetterOrDigit,
                RequireUppercase = passwordConfiguration.RequireUppercase,
            };
        }

        private class TestGlobalSettings : IGlobalSettings
        {
            public string ReservedUrls { get; set; }

            public string ReservedPaths { get; set; }

            public int TimeOutInMinutes { get; set; }

            public string DefaultUILanguage { get; set; }

            public bool HideTopLevelNodeFromPath { get; set; }

            public bool UseHttps { get; set; }

            public int VersionCheckPeriod { get; set; }

            public string UmbracoPath { get; set; }

            public string UmbracoCssPath { get; set; }

            public string UmbracoScriptsPath { get; set; }

            public string UmbracoMediaPath { get; set; }

            public bool IsSmtpServerConfigured { get; set; }

            public ISmtpSettings SmtpSettings { get; set; }

            public bool InstallMissingDatabase { get; set; }

            public bool InstallEmptyDatabase { get; set; }

            public bool DisableElectionForSingleServer { get; set; }

            public string RegisterType { get; set; }

            public string DatabaseFactoryServerVersion { get; set; }

            public string MainDomLock { get; set; }

            public string NoNodesViewPath { get; set; }

            public string IconsPath { get; set; }

            public string ConfigurationStatus { get; set; }
        }

        private class TestSmtpSettings : ISmtpSettings
        {
            public string From { get; set; }

            public string Host { get; set; }

            public int Port { get; set; }

            public string PickupDirectoryLocation { get; set; }

            public SmtpDeliveryMethod DeliveryMethod { get; set; }

            public string Username { get; set; }

            public string Password { get; set; }
        }

        private class TestConnectionStrings : IConnectionStrings
        {
            private IDictionary<string, ConfigConnectionString> _dictionary = new Dictionary<string, ConfigConnectionString>();

            public ConfigConnectionString this[string key] => _dictionary[key];

            public void AddEntry(string key, string connectionString)
            {
                _dictionary.Add(key, new ConfigConnectionString(connectionString, string.Empty, key));
            }
        }

        private class TestUserPasswordConfiguration : IUserPasswordConfiguration
        {
            public int RequiredLength { get; set; }

            public bool RequireNonLetterOrDigit { get; set; }

            public bool RequireDigit { get; set; }

            public bool RequireLowercase { get; set; }

            public bool RequireUppercase { get; set; }

            public string HashAlgorithmType { get; set; }

            public int MaxFailedAccessAttemptsBeforeLockout { get; set; }
        }
    }
}
