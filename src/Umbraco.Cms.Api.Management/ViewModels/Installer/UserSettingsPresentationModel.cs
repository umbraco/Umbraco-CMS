namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class UserSettingsPresentationModel
{
    public int MinCharLength { get; set; }

    public int MinNonAlphaNumericLength { get; set; }

    public IEnumerable<ConsentLevelPresentationModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelPresentationModel>();
}
