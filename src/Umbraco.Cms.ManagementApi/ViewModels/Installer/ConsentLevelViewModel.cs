using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.ViewModels.Installer;

[DataContract(Name = "consentLevels")]
public class ConsentLevelViewModel
{
    [DataMember(Name = "level")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryLevel Level { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; } = string.Empty;
}
