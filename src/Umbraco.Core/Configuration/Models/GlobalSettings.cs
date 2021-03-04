// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for global settings.
    /// </summary>
    public class GlobalSettings
    {
        internal const string
            StaticReservedPaths = "~/app_plugins/,~/install/,~/mini-profiler-resources/,~/umbraco/,"; // must end with a comma!

        internal const string
            StaticReservedUrls = "~/config/splashes/noNodes.aspx,~/.well-known,"; // must end with a comma!

        /// <summary>
        /// Gets or sets a value for the reserved URLs.
        /// </summary>
        public string ReservedUrls { get; set; } = StaticReservedUrls;

        /// <summary>
        /// Gets or sets a value for the reserved paths.
        /// </summary>
        public string ReservedPaths { get; set; } = StaticReservedPaths;

        /// <summary>
        /// Gets or sets a value for the timeout in minutes.
        /// </summary>
        public int TimeOutInMinutes { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value for the default UI language.
        /// </summary>
        public string DefaultUILanguage { get; set; } = "en-US";

        /// <summary>
        /// Gets or sets a value indicating whether to hide the top level node from the path.
        /// </summary>
        public bool HideTopLevelNodeFromPath { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether HTTPS should be used.
        /// </summary>
        public bool UseHttps { get; set; } = false;

        /// <summary>
        /// Gets or sets a value for the version check period in days.
        /// </summary>
        public int VersionCheckPeriod { get; set; } = 7;

        /// <summary>
        /// Gets or sets a value for the Umbraco back-office path.
        /// </summary>
        public string UmbracoPath { get; set; } = "~/umbraco";

        /// <summary>
        /// Gets or sets a value for the Umbraco icons path.
        /// </summary>
        /// <remarks>
        /// TODO: Umbraco cannot be hard coded here that is what UmbracoPath is for
        ///       so this should not be a normal get set it has to have dynamic ability to return the correct
        ///       path given UmbracoPath if this hasn't been explicitly set.
        /// </remarks>
        public string IconsPath { get; set; } = $"~/umbraco/assets/icons";

        /// <summary>
        /// Gets or sets a value for the Umbraco CSS path.
        /// </summary>
        public string UmbracoCssPath { get; set; } = "~/css";

        /// <summary>
        /// Gets or sets a value for the Umbraco scripts path.
        /// </summary>
        public string UmbracoScriptsPath { get; set; } = "~/scripts";

        /// <summary>
        /// Gets or sets a value for the Umbraco media path.
        /// </summary>
        public string UmbracoMediaPath { get; set; } = "~/media";

        /// <summary>
        /// Gets or sets a value indicating whether to install the database when it is missing.
        /// </summary>
        public bool InstallMissingDatabase { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether unattended installs are enabled.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured and it is possible to connect to
        /// the database, but the database is empty, the runtime enters the <c>Install</c> level.
        /// If this option is set to <c>true</c> an unattended install will be performed and the runtime enters
        /// the <c>Run</c> level.</para>
        /// </remarks>
        public bool InstallUnattended { get; set; } = false;

        /// <summary>
        /// Gets or sets a value to use for creating a user with a name for Unattended Installs
        /// </summary>
        public string UnattendedUserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value to use for creating a user with an email for Unattended Installs
        /// </summary>
        public string UnattendedUserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value to use for creating a user with a password for Unattended Installs
        /// </summary>
        public string UnattendedUserPassword { get; set; } = string.Empty;


        /// <summary>
        /// Gets or sets a value indicating whether to disable the election for a single server.
        /// </summary>
        public bool DisableElectionForSingleServer { get; set; } = false;

        /// <summary>
        /// Gets or sets a value for the database factory server version.
        /// </summary>
        public string DatabaseFactoryServerVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for the main dom lock.
        /// </summary>
        public string MainDomLock { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value for the path to the no content view.
        /// </summary>
        public string NoNodesViewPath { get; set; } = "~/umbraco/UmbracoWebsite/NoNodes.cshtml";

        /// <summary>
        /// Gets or sets a value for the database server registrar settings.
        /// </summary>
        public DatabaseServerRegistrarSettings DatabaseServerRegistrar { get; set; } = new DatabaseServerRegistrarSettings();

        /// <summary>
        /// Gets or sets a value for the database server messenger settings.
        /// </summary>
        public DatabaseServerMessengerSettings DatabaseServerMessenger { get; set; } = new DatabaseServerMessengerSettings();

        /// <summary>
        /// Gets or sets a value for the SMTP settings.
        /// </summary>
        public SmtpSettings Smtp { get; set; }

        /// <summary>
        /// Gets a value indicating whether SMTP is configured.
        /// </summary>
        public bool IsSmtpServerConfigured => !string.IsNullOrWhiteSpace(Smtp?.Host);
    }
}
