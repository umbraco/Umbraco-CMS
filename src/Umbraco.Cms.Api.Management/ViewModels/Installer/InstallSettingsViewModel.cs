namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class InstallSettingsViewModel
{
    public UserSettingsViewModel User { get; set; } = null!;

    public IEnumerable<DatabaseSettingsViewModel> Databases { get; set; } = Enumerable.Empty<DatabaseSettingsViewModel>();
}
