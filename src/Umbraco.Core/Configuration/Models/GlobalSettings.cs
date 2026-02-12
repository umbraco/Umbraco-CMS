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
    /// <summary>
    ///     The default value for the <see cref="ReservedPaths" /> setting.
    /// </summary>
    /// <remarks>Must end with a comma.</remarks>
    internal const string StaticReservedPaths =
        "~/app_plugins/,~/install/,~/mini-profiler-resources/,~/umbraco/,";

    /// <summary>
    ///     The default value for the <see cref="ReservedUrls" /> setting.
    /// </summary>
    /// <remarks>Must end with a comma.</remarks>
    internal const string StaticReservedUrls = "~/.well-known,";

    /// <summary>
    ///     The default value for the <see cref="TimeOut" /> setting.
    /// </summary>
    internal const string StaticTimeOut = "00:20:00";

    /// <summary>
    ///     The default value for the <see cref="DefaultUILanguage" /> setting.
    /// </summary>
    internal const string StaticDefaultUILanguage = "en-US";

    /// <summary>
    ///     The default value for the <see cref="HideTopLevelNodeFromPath" /> setting.
    /// </summary>
    internal const bool StaticHideTopLevelNodeFromPath = true;

    /// <summary>
    ///     The default value for the <see cref="UseHttps" /> setting.
    /// </summary>
    internal const bool StaticUseHttps = true;

    /// <summary>
    ///     The default value for the <see cref="VersionCheckPeriod" /> setting.
    /// </summary>
    internal const int StaticVersionCheckPeriod = 7;

    /// <summary>
    ///     The default value for the <see cref="IconsPath" /> setting.
    /// </summary>
    internal const string StaticIconsPath = "umbraco/assets/icons";

    /// <summary>
    ///     The default value for the <see cref="UmbracoCssPath" /> setting.
    /// </summary>
    internal const string StaticUmbracoCssPath = "~/css";

    /// <summary>
    ///     The default value for the <see cref="UmbracoScriptsPath" /> setting.
    /// </summary>
    internal const string StaticUmbracoScriptsPath = "~/scripts";

    /// <summary>
    ///     The default value for the <see cref="UmbracoMediaPath" /> setting.
    /// </summary>
    internal const string StaticUmbracoMediaPath = "~/media";

    /// <summary>
    ///     The default value for the <see cref="DisableElectionForSingleServer" /> setting.
    /// </summary>
    internal const bool StaticDisableElectionForSingleServer = false;

    /// <summary>
    ///     The default value for the <see cref="NoNodesViewPath" /> setting.
    /// </summary>
    internal const string StaticNoNodesViewPath = "~/umbraco/UmbracoWebsite/NoNodes.cshtml";

    /// <summary>
    ///     The default value for the <see cref="DistributedLockingReadLockDefaultTimeout" /> setting.
    /// </summary>
    internal const string StaticDistributedLockingReadLockDefaultTimeout = "00:01:00";

    /// <summary>
    ///     The default value for the <see cref="DistributedLockingWriteLockDefaultTimeout" /> setting.
    /// </summary>
    internal const string StaticDistributedLockingWriteLockDefaultTimeout = "00:00:05";

    /// <summary>
    ///     The default value for the <see cref="MainDomReleaseSignalPollingInterval" /> setting.
    /// </summary>
    internal const int StaticMainDomReleaseSignalPollingInterval = 2000;

    private const bool StaticForceCombineUrlPathLeftToRight = true;
    private const bool StaticShowMaintenancePageWhenInUpgradeState = true;

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
    ///     Gets a value indicating whether SMTP expiry is configured.
    /// </summary>
    public bool IsSmtpExpiryConfigured => Smtp?.EmailExpiration != null && Smtp?.EmailExpiration.HasValue == true;

    /// <summary>
    ///     Gets a value indicating whether there is a physical pickup directory configured.
    /// </summary>
    public bool IsPickupDirectoryLocationConfigured => !string.IsNullOrWhiteSpace(Smtp?.PickupDirectoryLocation);

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

    /// <summary>
    ///     Gets or sets a value indicating whether to show the maintenance page when in an upgrade state.
    /// </summary>
    [DefaultValue(StaticShowMaintenancePageWhenInUpgradeState)]
    public bool ShowMaintenancePageWhenInUpgradeState { get; set; } = StaticShowMaintenancePageWhenInUpgradeState;
}
