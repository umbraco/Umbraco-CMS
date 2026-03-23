using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the data required to perform an installation via the installer API.
/// </summary>
public class InstallRequestModel
{
    /// <summary>
    /// Gets or sets the information required to create the initial user during installation.
    /// </summary>
    [Required]
    public UserInstallRequestModel User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the configuration details for the database to be used during installation.
    /// </summary>
    [Required]
    public DatabaseInstallRequestModel Database { get; set; } = null!;

    /// <summary>
    /// Gets or sets the level of telemetry data to be collected during installation.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel TelemetryLevel { get; set; } = TelemetryLevel.Basic;
}
