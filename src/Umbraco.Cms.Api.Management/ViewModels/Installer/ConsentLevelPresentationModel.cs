using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class ConsentLevelPresentationModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel Level { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;
}
