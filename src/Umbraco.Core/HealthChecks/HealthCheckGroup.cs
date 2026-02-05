using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.HealthChecks;

/// <summary>
///     Represents a group of related health checks.
/// </summary>
[DataContract(Name = "healthCheckGroup", Namespace = "")]
public class HealthCheckGroup
{
    /// <summary>
    ///     Gets or sets the name of the health check group.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the list of health checks in this group.
    /// </summary>
    [DataMember(Name = "checks")]
    public List<HealthCheck>? Checks { get; set; }
}
