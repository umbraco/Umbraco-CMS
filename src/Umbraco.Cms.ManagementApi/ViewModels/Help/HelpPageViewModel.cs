using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.Help;

public class HelpPageViewModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
