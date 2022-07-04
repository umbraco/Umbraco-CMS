using Umbraco.Cms.BackOfficeApi.Models.Installer;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.BackOfficeApi.Factories.Installer;

public class InstallSettingsFactory : IInstallSettingsFactory
{
    private readonly IUserSettingsFactory _userSettingsFactory;
    private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata;

    public InstallSettingsFactory(
        IUserSettingsFactory userSettingsFactory,
        IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata)
    {
        _userSettingsFactory = userSettingsFactory;
        _databaseProviderMetadata = databaseProviderMetadata;
    }

    public InstallSettingsModel GetInstallSettings() =>
        new InstallSettingsModel
        {
            DatabaseSettings = _databaseProviderMetadata.GetAvailable(),
            UserSettings = _userSettingsFactory.GetUserSettings(),
        };
}
