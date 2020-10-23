namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    ///     The GlobalSettings Class contains general settings information for the entire Umbraco instance based on information
    ///     from  web.config appsettings
    /// </summary>
    public class GlobalSettings
    {
        internal const string
            StaticReservedPaths = "~/app_plugins/,~/install/,~/mini-profiler-resources/,~/umbraco/,"; //must end with a comma!

        internal const string
            StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; //must end with a comma!

        public string ReservedUrls { get; set; } = StaticReservedUrls;

        public string ReservedPaths { get; set; } = StaticReservedPaths;

        // TODO: https://github.com/umbraco/Umbraco-CMS/issues/4238 - stop having version in web.config appSettings
        // TODO: previously this would throw on set, but presumably we can't do that if we do still want this in config.
        public string ConfigurationStatus { get; set; }

        public int TimeOutInMinutes { get; set; } = 20;

        public string DefaultUILanguage { get; set; } = "en-US";

        public bool HideTopLevelNodeFromPath { get; set; } = false;

        public bool UseHttps { get; set; } = false;

        public int VersionCheckPeriod { get; set; } = 7;

        public string UmbracoPath { get; set; } = "~/umbraco";

        // TODO: Umbraco cannot be hard coded here that is what UmbracoPath is for
        // so this should not be a normal get set it has to have dynamic ability to return the correct
        // path given UmbracoPath if this hasn't been explicitly set.
        public string IconsPath { get; set; } = $"~/umbraco/assets/icons";

        public string UmbracoCssPath { get; set; } = "~/css";

        public string UmbracoScriptsPath { get; set; } = "~/scripts";

        public string UmbracoMediaPath { get; set; } = "~/media";

        public bool InstallMissingDatabase { get; set; } = false;

        public bool InstallEmptyDatabase { get; set; } = false;

        public bool DisableElectionForSingleServer { get; set; } = false;

        public string RegisterType { get; set; } = string.Empty;

        public string DatabaseFactoryServerVersion { get; set; } = string.Empty;

        public string MainDomLock { get; set; } = string.Empty;

        public string NoNodesViewPath { get; set; } = "~/config/splashes/NoNodes.cshtml";

        public bool IsSmtpServerConfigured => !string.IsNullOrWhiteSpace(Smtp?.Host);

        public SmtpSettings Smtp { get; set; }
    }
}
