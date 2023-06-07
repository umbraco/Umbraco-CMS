using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Factories;

public interface IInstallSettingsFactory
{
    InstallSettingsModel GetInstallSettings();
}
