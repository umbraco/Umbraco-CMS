using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Telemetry;

/// <summary>
/// Serves as the base class for view models that represent telemetry data in the API.
/// </summary>
public class TelemetryRepresentationBase
{
    /// <summary>
    /// Gets or sets the level of telemetry data collection for this representation.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel TelemetryLevel { get; set; }
}
