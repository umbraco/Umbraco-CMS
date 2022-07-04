using System.Runtime.Serialization;

namespace Umbraco.Cms.BackOfficeApi.ViewModels.Installer;

[DataContract(Name = "installSettings")]
public class InstallSettingsViewModel
{
    [DataMember(Name = "user")]
    public UserSettingsViewModel User { get; set; } = null!;

    [DataMember(Name = "databases")]
    public IEnumerable<DatabaseSettingsViewModel> Databases { get; set; } = Enumerable.Empty<DatabaseSettingsViewModel>();
}
