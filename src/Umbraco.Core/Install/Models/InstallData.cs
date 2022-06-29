using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Install.Models;

[DataContract(Name = "installData")]
public class InstallData
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

    [DataMember(Name = "telemetryLevel")]
    public TelemetryLevel TelemetryLevel { get; init; } = TelemetryLevel.Basic;

    [DataMember(Name = "database")]
    [Required]
    public DatabaseModel Database { get; init; } = null!;

    // public IEnumerable<IInstallMetaData> Metadata { get; set; }
}
