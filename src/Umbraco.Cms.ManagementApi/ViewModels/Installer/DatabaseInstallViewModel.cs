using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

public class DatabaseInstallViewModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public string? ProviderName { get; init; }

    public string? Server { get; init; }

    public string? Name { get; init; }

    public string? Username { get; init; }

    [PasswordPropertyText]
    public string? Password { get; init; }

    public bool UseIntegratedAuthentication { get; init; }

    public string? ConnectionString { get; init; }
}
