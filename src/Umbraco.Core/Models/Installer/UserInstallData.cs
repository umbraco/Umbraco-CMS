namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the user data provided during the installation process.
/// </summary>
public class UserInstallData
{
    /// <summary>
    ///     Gets or sets the name of the user.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the password for the user.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a value indicating whether the user wants to subscribe to the newsletter.
    /// </summary>
    public bool SubscribeToNewsletter { get; set; }
}
