using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

public class JsonPatchViewModel
{
    [JsonPropertyName("op")]
    public string? Op { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}
