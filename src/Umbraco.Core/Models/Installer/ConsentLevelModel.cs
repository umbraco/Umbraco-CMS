using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents a telemetry consent level option available during installation.
/// </summary>
public class ConsentLevelModel
{
    /// <summary>
    ///     Gets or sets the telemetry level.
    /// </summary>
    public TelemetryLevel Level { get; set; }

    /// <summary>
    ///     Gets or sets the description of the telemetry level.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
