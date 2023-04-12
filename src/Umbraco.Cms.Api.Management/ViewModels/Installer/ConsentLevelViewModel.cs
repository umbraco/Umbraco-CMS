using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.Installer;

public class ConsentLevelViewModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel Level { get; set; }

    public string Description { get; set; } = string.Empty;
}
