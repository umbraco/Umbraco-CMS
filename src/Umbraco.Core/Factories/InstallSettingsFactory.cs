using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory for creating <see cref="InstallSettingsModel"/> instances containing installation settings.
/// </summary>
public class InstallSettingsFactory : IInstallSettingsFactory
{
    private readonly IUserSettingsFactory _userSettingsFactory;
    private readonly IDatabaseSettingsFactory _databaseSettingsFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallSettingsFactory"/> class.
    /// </summary>
    /// <param name="userSettingsFactory">The factory for user settings.</param>
    /// <param name="databaseSettingsFactory">The factory for database settings.</param>
    public InstallSettingsFactory(
        IUserSettingsFactory userSettingsFactory,
        IDatabaseSettingsFactory databaseSettingsFactory)
    {
        _userSettingsFactory = userSettingsFactory;
        _databaseSettingsFactory = databaseSettingsFactory;
    }

    /// <inheritdoc />
    public InstallSettingsModel GetInstallSettings() =>
        new()
        {
            DatabaseSettings = _databaseSettingsFactory.GetDatabaseSettings(),
            UserSettings = _userSettingsFactory.GetUserSettings(),
        };
}
