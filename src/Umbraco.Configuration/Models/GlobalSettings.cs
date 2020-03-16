using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Umbraco.Composing;
using Umbraco.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Configuration.Models
{
    /// <summary>
    /// The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public class GlobalSettings : IGlobalSettings
    {
        private readonly IConfiguration _configuration;

        internal const string StaticReservedPaths = "~/app_plugins/,~/install/,~/mini-profiler-resources/,"; //must end with a comma!
        internal const string StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; //must end with a comma!
        public GlobalSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ReservedUrls => _configuration.GetValue("Umbraco:CMS:Global:ReservedUrls", StaticReservedUrls);
        public string ReservedPaths => _configuration.GetValue("Umbraco:CMS:Global:ReservedPaths", StaticReservedPaths);
        public string Path  => _configuration.GetValue<string>("Umbraco:CMS:Global:Path");
        // TODO: https://github.com/umbraco/Umbraco-CMS/issues/4238 - stop having version in web.config appSettings
        public string ConfigurationStatus
        {
            get => _configuration.GetValue<string>("Umbraco:CMS:Global:ConfigurationStatus");
            set => throw new NotImplementedException("We should remove this and only use the value from database");
        }

        public int TimeOutInMinutes => _configuration.GetValue("Umbraco:CMS:Global:TimeOutInMinutes", 20);
        public string DefaultUILanguage => _configuration.GetValue("Umbraco:CMS:Global:TimeOutInMinutes", "en-US");
        public bool HideTopLevelNodeFromPath => _configuration.GetValue("Umbraco:CMS:Global:HideTopLevelNodeFromPath", false);
        public bool UseHttps => _configuration.GetValue("Umbraco:CMS:Global:UseHttps", false);
        public int VersionCheckPeriod => _configuration.GetValue("Umbraco:CMS:Global:VersionCheckPeriod", 7);
        public string UmbracoPath => _configuration.GetValue("Umbraco:CMS:Global:UmbracoPath", "~/umbraco");
        public string UmbracoCssPath => _configuration.GetValue("Umbraco:CMS:Global:UmbracoCssPath", "~/css");
        public string UmbracoScriptsPath => _configuration.GetValue("Umbraco:CMS:Global:UmbracoScriptsPath", "~/scripts");
        public string UmbracoMediaPath => _configuration.GetValue("Umbraco:CMS:Global:UmbracoMediaPath", "~/media");
        public bool InstallMissingDatabase => _configuration.GetValue("Umbraco:CMS:Global:InstallMissingDatabase", false);
        public bool InstallEmptyDatabase => _configuration.GetValue("Umbraco:CMS:Global:InstallEmptyDatabase", false);
        public bool DisableElectionForSingleServer => _configuration.GetValue("Umbraco:CMS:Global:DisableElectionForSingleServer", false);
        public string RegisterType => _configuration.GetValue("Umbraco:CMS:Global:RegisterType", string.Empty);
        public string DatabaseFactoryServerVersion => _configuration.GetValue("Umbraco:CMS:Global:DatabaseFactoryServerVersion", string.Empty);
        public string MainDomLock => _configuration.GetValue("Umbraco:CMS:Global:MainDomLock", string.Empty);
        public string NoNodesViewPath => _configuration.GetValue("Umbraco:CMS:Global:NoNodesViewPath", "~/config/splashes/NoNodes.cshtml");

        public bool IsSmtpServerConfigured =>
            _configuration.GetSection("Umbraco:CMS:Smtp")?.GetChildren().Any() ?? false;

        public ISmtpSettings SmtpSettings => new SmtpSettingsImpl(_configuration.GetSection("Umbraco:CMS:Smtp"));

        private class SmtpSettingsImpl : ISmtpSettings
        {
            private readonly IConfigurationSection _configurationSection;

            public SmtpSettingsImpl(IConfigurationSection configurationSection)
            {
                _configurationSection = configurationSection;
            }

            public string From => _configurationSection.GetValue<string>("From");
            public string Host  => _configurationSection.GetValue<string>("Host");
            public int Port  => _configurationSection.GetValue<int>("Port");
            public string PickupDirectoryLocation  => _configurationSection.GetValue<string>("PickupDirectoryLocation");
        }
    }
}
