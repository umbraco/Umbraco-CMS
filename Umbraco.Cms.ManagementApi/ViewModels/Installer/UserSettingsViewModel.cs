using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

[DataContract(Name = "user")]
public class UserSettingsViewModel
{
    [DataMember(Name = "minCharLength")]
    public int MinCharLength { get; set; }

    [DataMember(Name = "minNonAlphaNumericLength")]
    public int MinNonAlphaNumericLength { get; set; }

    [DataMember(Name = "consentLevels")]
    public IEnumerable<ConsentLevelViewModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelViewModel>();
}
