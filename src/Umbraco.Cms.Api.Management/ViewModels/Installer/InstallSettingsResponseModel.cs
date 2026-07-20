using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the response model containing configuration settings returned during the installation process of Umbraco CMS.
/// </summary>
public class InstallSettingsResponseModel
{
    /// <summary>
    /// Gets or sets the presentation model containing user settings for the installation process.
    /// </summary>
    [Required]
    public UserSettingsPresentationModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of supported database settings for installation.
    /// </summary>
    public IEnumerable<DatabaseSettingsPresentationModel> Databases { get; set; } = Enumerable.Empty<DatabaseSettingsPresentationModel>();
}
