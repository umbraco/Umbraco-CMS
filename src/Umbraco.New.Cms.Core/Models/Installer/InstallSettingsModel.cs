namespace Umbraco.New.Cms.Core.Models.Installer;

public class InstallSettingsModel
{
    public UserSettingsModel UserSettings { get; set; } = null!;

    public ICollection<DatabaseSettingsModel> DatabaseSettings { get; set; } = new List<DatabaseSettingsModel>();
}
