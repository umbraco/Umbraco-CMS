using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.HealthChecks;

[DataContract(Name = "healthCheckGroup", Namespace = "")]
public class HealthCheckGroup
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    [DataMember(Name = "checks")]
    public List<HealthCheck>? Checks { get; set; }
}
