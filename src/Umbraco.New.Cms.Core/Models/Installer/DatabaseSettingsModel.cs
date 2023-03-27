namespace Umbraco.New.Cms.Core.Models.Installer;

public class DatabaseSettingsModel
{
    public Guid Id { get; set; }

    public int SortOrder { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string DefaultDatabaseName { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public bool IsConfigured { get; set; }

    public bool RequiresServer { get; set; }

    public string ServerPlaceholder { get; set; } = string.Empty;

    public bool RequiresCredentials { get; set; }

    public bool SupportsIntegratedAuthentication { get; set; }

    public bool RequiresConnectionTest { get; set; }
}
