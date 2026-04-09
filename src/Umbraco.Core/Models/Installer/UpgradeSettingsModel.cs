using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the settings for an upgrade operation, including version information.
/// </summary>
public class UpgradeSettingsModel
{
    /// <summary>
    ///     Gets or sets the current state of the installation before upgrade.
    /// </summary>
    public string CurrentState { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the new state of the installation after upgrade.
    /// </summary>
    public string NewState { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the new version being upgraded to.
    /// </summary>
    public SemVersion NewVersion { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the old version being upgraded from.
    /// </summary>
    public SemVersion OldVersion { get; set; } = null!;
}
