using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class UserInstallViewModel
{
    [DataMember(Name = "name")]
    [Required]
    [StringLength(255)]
    public string Name { get; init; } = null!;

    [DataMember(Name = "email")]
    [Required]
    [EmailAddress]
    public string Email { get; init; } = null!;

    [DataMember(Name = "password")]
    [Required]
    [PasswordPropertyText]
    public string Password { get; init; } = null!;

    [DataMember(Name = "subscribeToNewsletter")]
    public bool SubscribeToNewsletter { get; init; }
}
