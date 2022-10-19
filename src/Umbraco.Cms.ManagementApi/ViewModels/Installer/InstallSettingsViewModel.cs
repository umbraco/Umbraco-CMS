namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class InstallSettingsViewModel
{
    public UserSettingsViewModel User { get; set; } = null!;

    public IEnumerable<DatabaseSettingsViewModel> Databases { get; set; } = Enumerable.Empty<DatabaseSettingsViewModel>();
}
