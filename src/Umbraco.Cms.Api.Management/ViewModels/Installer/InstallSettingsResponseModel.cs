using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class InstallSettingsResponseModel
{
    [Required]
    public UserSettingsPresentationModel User { get; set; } = null!;

    public IEnumerable<DatabaseSettingsPresentationModel> Databases { get; set; } = Enumerable.Empty<DatabaseSettingsPresentationModel>();
}
