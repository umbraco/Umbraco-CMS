using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models
{
    [DataContract]
    public class TelemetryResource
    {
        [DataMember] public string TelemetryLevel { get; set; }
    }
}
