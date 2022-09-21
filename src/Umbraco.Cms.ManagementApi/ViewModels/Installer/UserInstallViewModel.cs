using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class UserInstallViewModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; } = null!;

    [Required]
    [PasswordPropertyText]
    public string Password { get; } = null!;

    public bool SubscribeToNewsletter { get; }
}
