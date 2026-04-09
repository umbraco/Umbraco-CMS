using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory interface for creating <see cref="UpgradeSettingsModel"/> instances.
/// </summary>
public interface IUpgradeSettingsFactory
{
    /// <summary>
    /// Gets the upgrade settings model containing version and migration state information.
    /// </summary>
    /// <returns>An <see cref="UpgradeSettingsModel"/> containing upgrade-related settings.</returns>
    UpgradeSettingsModel GetUpgradeSettings();
}
