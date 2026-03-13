using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

/// <summary>
/// Represents the data model used to present consent level options during the installation process.
/// </summary>
public class ConsentLevelPresentationModel
{
    /// <summary>
    /// Gets or sets the telemetry consent level.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel Level { get; set; }

    /// <summary>
    /// The description of the consent level.
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;
}
