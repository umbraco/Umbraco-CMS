namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the user-related settings available during installation.
/// </summary>
public class UserSettingsModel
{
    /// <summary>
    ///     Gets or sets the password settings configuration.
    /// </summary>
    public PasswordSettingsModel PasswordSettings { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the collection of consent levels for telemetry.
    /// </summary>
    public IEnumerable<ConsentLevelModel> ConsentLevels { get; set; } = Enumerable.Empty<ConsentLevelModel>();
}
