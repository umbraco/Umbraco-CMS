using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class DatabaseInstallRequestModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string? ProviderName { get; set; }

    public string? Server { get; set; }

    public string? Name { get; set; }

    public string? Username { get; set; }

    [PasswordPropertyText]
    public string? Password { get; set; }

    public bool UseIntegratedAuthentication { get; set; }

    public string? ConnectionString { get; set; }

    public bool TrustServerCertificate { get; set; }
}
