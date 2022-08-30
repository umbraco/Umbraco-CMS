using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

[DataContract(Name = "databaseInstall")]
public class DatabaseInstallViewModel
{
    [DataMember(Name = "id")]
    [Required]
    public Guid Id { get; init; }

    [DataMember(Name = "providerName")]
    [Required]
    public string? ProviderName { get; init; }

    [DataMember(Name = "server")]
    public string? Server { get; init; }

    [DataMember(Name = "name")]
    public string? Name { get; init; }

    [DataMember(Name = "username")]
    public string? Username { get; init; }

    [DataMember(Name = "password")]
    [PasswordPropertyText]
    public string? Password { get; init; }

    [DataMember(Name = "useIntegratedAuthentication")]
    public bool UseIntegratedAuthentication { get; init; }

    [DataMember(Name = "connectionString")]
    public string? ConnectionString { get; init; }
}
