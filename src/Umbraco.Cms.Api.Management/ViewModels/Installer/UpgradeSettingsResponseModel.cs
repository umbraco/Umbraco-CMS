namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class UpgradeSettingsResponseModel
{
    public string CurrentState { get; set; } = string.Empty;

    public string NewState { get; set; } = string.Empty;

    public string NewVersion { get; set; } = string.Empty;

    public string OldVersion { get; set; } = string.Empty;

    public string ReportUrl =>
        $"https://our.umbraco.com/contribute/releases/compare?from={OldVersion}&to={NewVersion}&notes=1";
}
