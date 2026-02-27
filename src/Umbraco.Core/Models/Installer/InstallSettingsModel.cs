namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the overall settings available during the installation process.
/// </summary>
public class InstallSettingsModel
{
    /// <summary>
    ///     Gets or sets the user-related settings.
    /// </summary>
    public UserSettingsModel UserSettings { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the collection of available database configurations.
    /// </summary>
    public ICollection<DatabaseSettingsModel> DatabaseSettings { get; set; } = new List<DatabaseSettingsModel>();
}
