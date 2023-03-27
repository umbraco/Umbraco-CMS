namespace Umbraco.New.Cms.Core.Models.Installer;

public class UserSettingsModel
{
    public PasswordSettingsModel PasswordSettings { get; set; } = null!;

    public IEnumerable<ConsentLevelModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelModel>();
}
