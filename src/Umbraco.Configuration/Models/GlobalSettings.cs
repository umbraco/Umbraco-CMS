using System;
using System.Linq;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    /// <summary>
    ///     The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information
    ///     from  web.config appsettings
    /// </summary>
    internal class GlobalSettings : IGlobalSettings
    {
        private const string Prefix = Constants.Configuration.ConfigGlobalPrefix;

        internal const string
            StaticReservedPaths = "~/app_plugins/,~/install/,~/mini-profiler-resources/,"; //must end with a comma!

        internal const string
            StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; //must end with a comma!

        private readonly IConfiguration _configuration;

        public GlobalSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ReservedUrls => _configuration.GetValue(Prefix + "ReservedUrls", StaticReservedUrls);
        public string ReservedPaths => _configuration.GetValue(Prefix + "ReservedPaths", StaticReservedPaths);

        // TODO: https://github.com/umbraco/Umbraco-CMS/issues/4238 - stop having version in web.config appSettings
        public string ConfigurationStatus
        {
            get => _configuration.GetValue<string>(Prefix + "ConfigurationStatus");
            set => throw new NotImplementedException("We should remove this and only use the value from database");
        }

        public int TimeOutInMinutes => _configuration.GetValue(Prefix + "TimeOutInMinutes", 20);
        public string DefaultUILanguage => _configuration.GetValue(Prefix + "DefaultUILanguage", "en-US");

        public bool HideTopLevelNodeFromPath =>
            _configuration.GetValue(Prefix + "HideTopLevelNodeFromPath", true);

        public bool UseHttps => _configuration.GetValue(Prefix + "UseHttps", false);
        public int VersionCheckPeriod => _configuration.GetValue(Prefix + "VersionCheckPeriod", 7);
        public string UmbracoPath => _configuration.GetValue(Prefix + "UmbracoPath", "~/umbraco");
        public string IconsPath => _configuration.GetValue(Prefix + "IconsPath", $"{UmbracoPath}/assets/icons");

        public string UmbracoCssPath => _configuration.GetValue(Prefix + "UmbracoCssPath", "~/css");

        public string UmbracoScriptsPath =>
            _configuration.GetValue(Prefix + "UmbracoScriptsPath", "~/scripts");

        public string UmbracoMediaPath => _configuration.GetValue(Prefix + "UmbracoMediaPath", "~/media");

        public bool InstallMissingDatabase =>
            _configuration.GetValue(Prefix + "InstallMissingDatabase", false);

        public bool InstallEmptyDatabase => _configuration.GetValue(Prefix + "InstallEmptyDatabase", false);

        public bool DisableElectionForSingleServer =>
            _configuration.GetValue(Prefix + "DisableElectionForSingleServer", false);

        public string RegisterType => _configuration.GetValue(Prefix + "RegisterType", string.Empty);

        public string DatabaseFactoryServerVersion =>
            _configuration.GetValue(Prefix + "DatabaseFactoryServerVersion", string.Empty);

        public string MainDomLock => _configuration.GetValue(Prefix + "MainDomLock", string.Empty);

        public string NoNodesViewPath =>
            _configuration.GetValue(Prefix + "NoNodesViewPath", "~/config/splashes/NoNodes.cshtml");

        public bool IsSmtpServerConfigured =>
            _configuration.GetSection(Constants.Configuration.ConfigGlobalPrefix + "Smtp")?.GetChildren().Any() ?? false;

        public ISmtpSettings SmtpSettings =>
            new SmtpSettingsImpl(_configuration.GetSection(Constants.Configuration.ConfigGlobalPrefix + "Smtp"));

        private class SmtpSettingsImpl : ISmtpSettings
        {
            private readonly IConfigurationSection _configurationSection;

            public SmtpSettingsImpl(IConfigurationSection configurationSection)
            {
                _configurationSection = configurationSection;
            }

            public string From => _configurationSection.GetValue<string>("From");
            public string Host => _configurationSection.GetValue<string>("Host");
            public int Port => _configurationSection.GetValue<int>("Port");
            public string PickupDirectoryLocation => _configurationSection.GetValue<string>("PickupDirectoryLocation");
            public SmtpDeliveryMethod DeliveryMethod => _configurationSection.GetValue<SmtpDeliveryMethod>("DeliveryMethod");

            public string Username => _configurationSection.GetValue<string>("Username");

            public string Password => _configurationSection.GetValue<string>("Password");
        }
    }
}
