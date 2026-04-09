using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the data provided during the installation process.
/// </summary>
public class InstallData
{
    /// <summary>
    ///     Gets or sets the user installation data.
    /// </summary>
    public UserInstallData User { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the database installation data.
    /// </summary>
    public DatabaseInstallData Database { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the telemetry consent level.
    /// </summary>
    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Detailed;
}
