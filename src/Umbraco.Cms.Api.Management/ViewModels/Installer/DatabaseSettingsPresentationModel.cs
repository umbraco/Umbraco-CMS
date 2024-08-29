using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class DatabaseSettingsPresentationModel
{
    public Guid Id { get; set; }

    public int SortOrder { get; set; }

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string DefaultDatabaseName { get; set; } = string.Empty;

    [Required]
    public string ProviderName { get; set; } = string.Empty;

    public bool IsConfigured { get; set; }

    public bool RequiresServer { get; set; }

    [Required]
    public string ServerPlaceholder { get; set; } = string.Empty;

    public bool RequiresCredentials { get; set; }

    public bool SupportsIntegratedAuthentication { get; set; }

    public bool RequiresConnectionTest { get; set; }
}
