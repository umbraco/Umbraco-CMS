using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("Will no longer be required with the new backoffice API")]
[DataContract(Name = "user", Namespace = "")]
public class UserModel
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = null!;

    [DataMember(Name = "email")]
    public string Email { get; set; } = null!;

    [DataMember(Name = "password")]
    public string Password { get; set; } = null!;

    [DataMember(Name = "subscribeToNewsLetter")]
    public bool SubscribeToNewsLetter { get; set; }

    [DataMember(Name = "telemetryLevel")]
    public TelemetryLevel TelemetryLevel { get; set; }
}
