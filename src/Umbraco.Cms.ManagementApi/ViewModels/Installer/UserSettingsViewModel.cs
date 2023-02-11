namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class UserSettingsViewModel
{
    public int MinCharLength { get; set; }

    public int MinNonAlphaNumericLength { get; set; }

    public IEnumerable<ConsentLevelViewModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelViewModel>();
}
