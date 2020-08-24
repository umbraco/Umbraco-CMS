using System.Net.Mail;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// TEMPORARY: this class has been added just to ensure tests on Umbraco.Web functionality, that still use the interface
    /// based configuration, by converting between e.g <see cref="GlobalSettings"></see> (used by
    /// legacy configuration and <see cref="IGlobalSettings"></see> (used by Netcore/IOptions configuration).
    /// </summary>
    public static class ConfigModelConversions
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
                UseHttps = globalSettings.UseHttps,
                VersionCheckPeriod = globalSettings.VersionCheckPeriod,
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
    }
}
