using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Install.Models;

[DataContract(Name = "installData")]
public class InstallData
{
    [DataMember(Name = "user")]
    [Required]
    public UserInstallData User { get; init; } = null!;

    [DataMember(Name = "database")]
    [Required]
    public DatabaseModel Database { get; init; } = null!;

    [DataMember(Name = "subscribeToNewsletter")]
    public bool SubscribeToNewsletter { get; init; }

    [DataMember(Name = "telemetryLevel")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel TelemetryLevel { get; init; } = TelemetryLevel.Basic;
}
