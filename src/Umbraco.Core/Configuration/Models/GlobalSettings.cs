// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for global settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigGlobal)]
public class GlobalSettings
{
    internal const string
        StaticReservedPaths =
            "~/app_plugins/,~/install/,~/mini-profiler-resources/,~/umbraco/,"; // must end with a comma!

    internal const string StaticReservedUrls = "~/.well-known,"; // must end with a comma!
    internal const string StaticTimeOut = "00:20:00";
    internal const string StaticDefaultUILanguage = "en-US";
    internal const bool StaticHideTopLevelNodeFromPath = true;
    internal const bool StaticUseHttps = false;
    internal const int StaticVersionCheckPeriod = 7;
    internal const string StaticIconsPath = "umbraco/assets/icons";
    internal const string StaticUmbracoCssPath = "~/css";
    internal const string StaticUmbracoScriptsPath = "~/scripts";
    internal const string StaticUmbracoMediaPath = "~/media";
    internal const bool StaticInstallMissingDatabase = false;
    internal const bool StaticDisableElectionForSingleServer = false;
    internal const string StaticNoNodesViewPath = "~/umbraco/UmbracoWebsite/NoNodes.cshtml";
    internal const string StaticDistributedLockingReadLockDefaultTimeout = "00:01:00";
    internal const string StaticDistributedLockingWriteLockDefaultTimeout = "00:00:05";
    internal const bool StaticSanitizeTinyMce = false;
    internal const int StaticMainDomReleaseSignalPollingInterval = 2000;
    private const bool StaticForceCombineUrlPathLeftToRight = true;

    /// <summary>
    ///     Gets or sets a value for the reserved URLs (must end with a comma).
    /// </summary>
    [DefaultValue(StaticReservedUrls)]
    public string ReservedUrls { get; set; } = StaticReservedUrls;

    /// <summary>
    ///     Gets or sets a value for the reserved paths (must end with a comma).
    /// </summary>
    [DefaultValue(StaticReservedPaths)]
    public string ReservedPaths { get; set; } = StaticReservedPaths;

    /// <summary>
    ///     Gets or sets a value for the back-office login timeout.
    /// </summary>
    [DefaultValue(StaticTimeOut)]
    public TimeSpan TimeOut { get; set; } = TimeSpan.Parse(StaticTimeOut);

    /// <summary>
    ///     Gets or sets a value for the default UI language.
    /// </summary>
    [DefaultValue(StaticDefaultUILanguage)]
    public string DefaultUILanguage { get; set; } = StaticDefaultUILanguage;

    /// <summary>
    ///     Gets or sets a value indicating whether to hide the top level node from the path.
    /// </summary>
    [DefaultValue(StaticHideTopLevelNodeFromPath)]
    public bool HideTopLevelNodeFromPath { get; set; } = StaticHideTopLevelNodeFromPath;

    /// <summary>
    ///     Gets or sets a value indicating whether HTTPS should be used.
    /// </summary>
    [DefaultValue(StaticUseHttps)]
    public bool UseHttps { get; set; } = StaticUseHttps;

    /// <summary>
    ///     Gets or sets a value for the version check period in days.
    /// </summary>
    [DefaultValue(StaticVersionCheckPeriod)]
    public int VersionCheckPeriod { get; set; } = StaticVersionCheckPeriod;

    /// <summary>
    ///     Gets or sets a value for the Umbraco back-office path.
    /// </summary>
    public string UmbracoPath
    {
        get => Constants.System.DefaultUmbracoPath;
        [Obsolete($"{nameof(UmbracoPath)}  is no longer configurable, property setter is scheduled for removal in V12")]
        // NOTE: when removing this, also clean up the hardcoded removal of UmbracoPath in UmbracoJsonSchemaGenerator
        set { }
    }

    /// <summary>
    ///     Gets or sets a value for the Umbraco icons path.
    /// </summary>
    /// <remarks>
    ///     TODO: Umbraco cannot be hard coded here that is what UmbracoPath is for
    ///     so this should not be a normal get set it has to have dynamic ability to return the correct
    ///     path given UmbracoPath if this hasn't been explicitly set.
    /// </remarks>
    [DefaultValue(StaticIconsPath)]
    public string IconsPath { get; set; } = StaticIconsPath;

    /// <summary>
    ///     Gets or sets a value for the Umbraco CSS path.
    /// </summary>
    [DefaultValue(StaticUmbracoCssPath)]
    public string UmbracoCssPath { get; set; } = StaticUmbracoCssPath;

    /// <summary>
    ///     Gets or sets a value for the Umbraco scripts path.
    /// </summary>
    [DefaultValue(StaticUmbracoScriptsPath)]
    public string UmbracoScriptsPath { get; set; } = StaticUmbracoScriptsPath;

    /// <summary>
    ///     Gets or sets a value for the Umbraco media request path.
    /// </summary>
    [DefaultValue(StaticUmbracoMediaPath)]
    public string UmbracoMediaPath { get; set; } = StaticUmbracoMediaPath;

    /// <summary>
    ///     Gets or sets a value for the physical Umbraco media root path (falls back to <see cref="UmbracoMediaPath" /> when
    ///     empty).
    /// </summary>
    /// <remarks>
    ///     If the value is a virtual path, it's resolved relative to the webroot.
    /// </remarks>
    public string UmbracoMediaPhysicalRootPath { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a value indicating whether to install the database when it is missing.
    /// </summary>
    [DefaultValue(StaticInstallMissingDatabase)]
    public bool InstallMissingDatabase { get; set; } = StaticInstallMissingDatabase;

    /// <summary>
    ///     Gets or sets a value indicating whether to disable the election for a single server.
    /// </summary>
    [DefaultValue(StaticDisableElectionForSingleServer)]
    public bool DisableElectionForSingleServer { get; set; } = StaticDisableElectionForSingleServer;

    /// <summary>
    ///     Gets or sets a value for the database factory server version.
    /// </summary>
    public string DatabaseFactoryServerVersion { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value for the main dom lock.
    /// </summary>
    public string MainDomLock { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value to discriminate MainDom boundaries.
    ///     <para>
    ///         Generally the default should suffice but useful for advanced scenarios e.g. azure deployment slot based zero
    ///         downtime deployments.
    ///     </para>
    /// </summary>
    public string MainDomKeyDiscriminator { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the duration (in milliseconds) for which the MainDomLock release signal polling task should sleep.
    /// </summary>
    /// <remarks>
    ///     Doesn't apply to MainDomSemaphoreLock.
    ///     <para>
    ///         The default value is 2000ms.
    ///     </para>
    /// </remarks>
    [DefaultValue(StaticMainDomReleaseSignalPollingInterval)]
    public int MainDomReleaseSignalPollingInterval { get; set; } = StaticMainDomReleaseSignalPollingInterval;

    /// <summary>
    ///     Gets or sets the telemetry ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value for the path to the no content view.
    /// </summary>
    [DefaultValue(StaticNoNodesViewPath)]
    public string NoNodesViewPath { get; set; } = StaticNoNodesViewPath;

    /// <summary>
    ///     Gets or sets a value for the database server registrar settings.
    /// </summary>
    public DatabaseServerRegistrarSettings DatabaseServerRegistrar { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value for the database server messenger settings.
    /// </summary>
    public DatabaseServerMessengerSettings DatabaseServerMessenger { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value for the SMTP settings.
    /// </summary>
    public SmtpSettings? Smtp { get; set; }

    /// <summary>
    ///     Gets a value indicating whether SMTP is configured.
    /// </summary>
    public bool IsSmtpServerConfigured => !string.IsNullOrWhiteSpace(Smtp?.Host);

    /// <summary>
    ///     Gets a value indicating whether there is a physical pickup directory configured.
    /// </summary>
    public bool IsPickupDirectoryLocationConfigured => !string.IsNullOrWhiteSpace(Smtp?.PickupDirectoryLocation);

    /// <summary>
    ///     Gets or sets a value indicating whether TinyMCE scripting sanitization should be applied.
    /// </summary>
    [DefaultValue(StaticSanitizeTinyMce)]
    public bool SanitizeTinyMce { get; set; } = StaticSanitizeTinyMce;

    /// <summary>
    ///     Gets or sets a value representing the maximum time to wait whilst attempting to obtain a distributed read lock.
    /// </summary>
    /// <remarks>
    ///     The default value is 60 seconds.
    /// </remarks>
    [DefaultValue(StaticDistributedLockingReadLockDefaultTimeout)]
    public TimeSpan DistributedLockingReadLockDefaultTimeout { get; set; } =
        TimeSpan.Parse(StaticDistributedLockingReadLockDefaultTimeout);

    /// <summary>
    ///     Gets or sets a value representing the maximum time to wait whilst attempting to obtain a distributed write lock.
    /// </summary>
    /// <remarks>
    ///     The default value is 5 seconds.
    /// </remarks>
    [DefaultValue(StaticDistributedLockingWriteLockDefaultTimeout)]
    public TimeSpan DistributedLockingWriteLockDefaultTimeout { get; set; } =
        TimeSpan.Parse(StaticDistributedLockingWriteLockDefaultTimeout);

    /// <summary>
    /// Gets or sets a value representing the DistributedLockingMechanism to use.
    /// </summary>
    public string DistributedLockingMechanism { get; set; } = string.Empty;

    /// <summary>
    /// Force url paths to be left to right, even when the culture has right to left text
    /// </summary>
    /// <example>
    /// For the following hierarchy
    /// - Root (/ar)
    ///   - 1 (/ar/1)
    ///     - 2 (/ar/1/2)
    ///       - 3 (/ar/1/2/3)
    ///         - 3 (/ar/1/2/3/4)
    /// When forced
    /// - https://www.umbraco.com/ar/1/2/3/4
    /// when not
    /// - https://www.umbraco.com/ar/4/3/2/1
    /// </example>
    [DefaultValue(StaticForceCombineUrlPathLeftToRight)]
    public bool ForceCombineUrlPathLeftToRight { get; set; }  = StaticForceCombineUrlPathLeftToRight;
}
