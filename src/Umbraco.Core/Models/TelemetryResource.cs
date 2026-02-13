using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a telemetry configuration resource.
/// </summary>
[DataContract]
public class TelemetryResource
{
    /// <summary>
    /// Gets or sets the telemetry level for the Umbraco installation.
    /// </summary>
    [DataMember]
    public TelemetryLevel TelemetryLevel { get; set; }
}
