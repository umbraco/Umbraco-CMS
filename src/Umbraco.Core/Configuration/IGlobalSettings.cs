namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Contains general settings information for the entire Umbraco instance based on information from  web.config appsettings
    /// </summary>
    public interface IGlobalSettings
    {
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
        /// Gets the path to umbraco's root directory (/umbraco by default).
        /// </summary>
        string Path { get; }

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
        /// Gets the configuration for the location of temporary files.
        /// </summary>
        LocalTempStorage LocalTempStorageLocation { get; }

        /// <summary>
        /// Gets the location of temporary files.
        /// </summary>
        string LocalTempPath { get; }

        /// <summary>
        /// Gets the write lock timeout.
        /// </summary>
        int SqlWriteLockTimeOut { get; }

        /// <summary>
        /// Returns true if TinyMCE scripting sanitization should be applied
        /// </summary>
        bool SanitizeTinyMce { get; }
    }
}
