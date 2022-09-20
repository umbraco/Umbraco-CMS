using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

[DataContract(Name = "upgradeSettingsViewModel")]
public class UpgradeSettingsViewModel
{
    [DataMember(Name = "currentState")]
    public string CurrentState { get; set; } = string.Empty;

    [DataMember(Name = "newState")]
    public string NewState { get; set; } = string.Empty;

    [DataMember(Name = "newVersion")]
    public string NewVersion { get; set; } = string.Empty;

    [DataMember(Name = "oldVersion")]
    public string OldVersion { get; set; } = string.Empty;

    [DataMember(Name = "reportUrl")]
    public string ReportUrl =>
        $"https://our.umbraco.com/contribute/releases/compare?from={OldVersion}&to={NewVersion}&notes=1";
}
