using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory interface for creating <see cref="InstallSettingsModel"/> instances.
/// </summary>
public interface IInstallSettingsFactory
{
    /// <summary>
    /// Gets the installation settings model containing database and user settings.
    /// </summary>
    /// <returns>An <see cref="InstallSettingsModel"/> containing installation settings.</returns>
    InstallSettingsModel GetInstallSettings();
}
