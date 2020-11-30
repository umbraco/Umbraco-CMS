namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public interface IGlobalSettings
    {
        // fixme: Review this class, it is now just a dumping ground for config options (based basically on whatever might be in appSettings),
        //          our config classes should be named according to what they are configuring.

        /// <summary>
        /// Gets the reserved URLs from web.config.
        /// </summary>
        /// <value>The reserved URLs.</value>
        string ReservedUrls { get; }

        /// <summary>
        /// Gets the reserved paths from web.config
        /// </summary>
        /// <value>The reserved paths.</value>
        string ReservedPaths { get; }

        /// <summary>
        /// Gets the path to umbraco's icons directory (/umbraco/assets/icons by default).
        /// </summary>
        string IconsPath { get; }

        /// <summary>
        /// Gets or sets the configuration status. This will return the version number of the currently installed umbraco instance.
        /// </summary>
        string ConfigurationStatus { get; set; }

        /// <summary>
        /// Gets the time out in minutes.
        /// </summary>
        int TimeOutInMinutes { get; }

        /// <summary>
        /// Gets the default UI language.
        /// </summary>
        /// <value>The default UI language.</value>
        // ReSharper disable once InconsistentNaming
        string DefaultUILanguage { get; }

        /// <summary>
        /// Gets a value indicating whether umbraco should hide top level nodes from generated URLs.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if umbraco hides top level nodes from URLs; otherwise, <c>false</c>.
        /// </value>
        bool HideTopLevelNodeFromPath { get; }

        /// <summary>
        /// Gets a value indicating whether umbraco should force a secure (https) connection to the backoffice.
        /// </summary>
        bool UseHttps { get; }

        /// <summary>
        /// Returns a string value to determine if umbraco should skip version-checking.
        /// </summary>
        /// <value>The version check period in days (0 = never).</value>
        int VersionCheckPeriod { get; }

        /// <summary>
        /// Gets the path to umbraco's root directory.
        /// </summary>
        string UmbracoPath { get; }
        string UmbracoCssPath { get; }
        string UmbracoScriptsPath { get; }
        string UmbracoMediaPath { get; }

        bool IsSmtpServerConfigured { get; }
        ISmtpSettings SmtpSettings { get; }

        /// <summary>
        /// Gets a value indicating whether the runtime should enter Install level when the database is missing.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured but it is not possible to
        /// connect to the database, the runtime enters the BootFailed level. If this options is set to true,
        /// it enters the Install level instead.</para>
        /// <para>It is then up to the implementor, that is setting this value, to take over the installation
        /// sequence.</para>
        /// </remarks>
        bool InstallMissingDatabase { get; }

        /// <summary>
        /// Gets a value indicating whether the runtime should enter Install level when the database is empty.
        /// </summary>
        /// <remarks>
        /// <para>By default, when a database connection string is configured and it is possible to connect to
        /// the database, but the database is empty, the runtime enters the BootFailed level. If this options
        /// is set to true, it enters the Install level instead.</para>
        /// <para>It is then up to the implementor, that is setting this value, to take over the installation
        /// sequence.</para>
        /// </remarks>
        bool InstallEmptyDatabase { get; }
        bool DisableElectionForSingleServer { get; }
        string RegisterType { get; }
        string DatabaseFactoryServerVersion { get; }
        string MainDomLock { get; }

        /// <summary>
        /// Gets the path to the razor file used when no published content is available.
        /// </summary>
        string NoNodesViewPath { get; }
    }
}
