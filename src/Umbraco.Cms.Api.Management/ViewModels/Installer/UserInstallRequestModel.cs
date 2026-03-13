using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the data required to create the initial user during the installation process.
/// </summary>
public class UserInstallRequestModel
{
    /// <summary>
    /// Gets or sets the name of the user to be installed.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password to be used for the user during installation.
    /// </summary>
    [Required]
    [PasswordPropertyText]
    public string Password { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the user wants to subscribe to the newsletter.</summary>
    public bool SubscribeToNewsletter { get; set; }
}
