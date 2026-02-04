using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory interface for creating <see cref="UserSettingsModel"/> instances.
/// </summary>
public interface IUserSettingsFactory
{
    /// <summary>
    /// Gets the user settings model containing password settings and consent levels.
    /// </summary>
    /// <returns>A <see cref="UserSettingsModel"/> containing user-related settings.</returns>
    UserSettingsModel GetUserSettings();
}
