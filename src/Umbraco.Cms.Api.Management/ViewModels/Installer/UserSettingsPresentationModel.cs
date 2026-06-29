namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents user settings in the installer view model.
/// </summary>
public class UserSettingsPresentationModel
{
    /// <summary>
    /// Gets or sets the minimum allowed character length for the user setting value.
    /// </summary>
    public int MinCharLength { get; set; }

    /// <summary>Gets or sets the minimum number of non-alphanumeric characters required.</summary>
    public int MinNonAlphaNumericLength { get; set; }

    /// <summary>
    /// Gets or sets the consent levels for the user settings.
    /// </summary>
    public IEnumerable<ConsentLevelPresentationModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelPresentationModel>();
}
