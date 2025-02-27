using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

public class InstallSettingsFactory : IInstallSettingsFactory
{
    private readonly IUserSettingsFactory _userSettingsFactory;
    private readonly IDatabaseSettingsFactory _databaseSettingsFactory;

    public InstallSettingsFactory(
        IUserSettingsFactory userSettingsFactory,
        IDatabaseSettingsFactory databaseSettingsFactory)
    {
        _userSettingsFactory = userSettingsFactory;
        _databaseSettingsFactory = databaseSettingsFactory;
    }

    public InstallSettingsModel GetInstallSettings() =>
        new()
        {
            DatabaseSettings = _databaseSettingsFactory.GetDatabaseSettings(),
            UserSettings = _userSettingsFactory.GetUserSettings(),
        };
}
