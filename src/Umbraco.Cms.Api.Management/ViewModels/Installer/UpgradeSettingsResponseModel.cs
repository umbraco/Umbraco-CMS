using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the response model containing upgrade settings information returned by the installer.
/// </summary>
public class UpgradeSettingsResponseModel
{
    /// <summary>
    /// Gets or sets the current state of the upgrade process.
    /// </summary>
    [Required]
    public string CurrentState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the new state of the installation or upgrade process after the upgrade operation completes.
    /// </summary>
    [Required]
    public string NewState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version string representing the new version available for upgrade.
    /// </summary>
    [Required]
    public string NewVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old version string before upgrade.
    /// </summary>
    [Required]
    public string OldVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets the URL for the upgrade report comparing the old and new versions.
    /// </summary>
    public string ReportUrl =>
        $"https://releases.umbraco.com/compare?from={OldVersion}&to={NewVersion}&notes=1";
}
