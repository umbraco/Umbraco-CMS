using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class UserInstallViewModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [PasswordPropertyText]
    public string Password { get; set; } = null!;

    public bool SubscribeToNewsletter { get; }
}
