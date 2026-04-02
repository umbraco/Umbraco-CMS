using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Defines the level of telemetry data to be collected and sent.
/// </summary>
[DataContract]
public enum TelemetryLevel
{
    /// <summary>
    /// Minimal telemetry - only essential anonymous usage data.
    /// </summary>
    Minimal,

    /// <summary>
    /// Basic telemetry - includes minimal data plus general feature usage statistics.
    /// </summary>
    Basic,

    /// <summary>
    /// Detailed telemetry - includes basic data plus detailed usage patterns and diagnostics.
    /// </summary>
    Detailed,
}
