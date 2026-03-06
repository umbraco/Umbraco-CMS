namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the password policy settings for user account creation.
/// </summary>
public class PasswordSettingsModel
{
    /// <summary>
    ///     Gets or sets the minimum character length required for passwords.
    /// </summary>
    public int MinCharLength { get; set; }

    /// <summary>
    ///     Gets or sets the minimum number of non-alphanumeric characters required in passwords.
    /// </summary>
    public int MinNonAlphaNumericLength { get; set; }
}
