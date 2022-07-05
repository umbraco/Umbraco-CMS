using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

[DataContract(Name = "databaseSettings")]
public class DatabaseSettingsViewModel
{
    [DataMember(Name = "id")]
    public Guid Id { get; set; }

    [DataMember(Name = "sortOrder")]
    public int SortOrder { get; set; }

    [DataMember(Name = "displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [DataMember(Name = "defaultDatabaseName")]
    public string DefaultDatabaseName { get; set; } = string.Empty;

    [DataMember(Name = "providerName")]
    public string ProviderName { get; set; } = string.Empty;

    [DataMember(Name = "isConfigured")]
    public bool IsConfigured { get; set; }

    [DataMember(Name = "requiresServer")]
    public bool RequiresServer { get; set; }

    [DataMember(Name = "serverPlaceholder")]
    public string ServerPlaceholder { get; set; } = string.Empty;

    [DataMember(Name = "requiresCredentials")]
    public bool RequiresCredentials { get; set; }

    [DataMember(Name = "supportsIntegratedAuthentication")]
    public bool SupportsIntegratedAuthentication { get; set; }

    [DataMember(Name = "requiresConnectionTest")]
    public bool RequiresConnectionTest { get; set; }
}
