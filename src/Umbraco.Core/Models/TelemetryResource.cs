using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

[DataContract]
public class TelemetryResource
{
    [DataMember]
    public TelemetryLevel TelemetryLevel { get; set; }
}
