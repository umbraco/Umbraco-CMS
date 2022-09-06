using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.JsonPatch;

public class JsonPatchViewModel
{
    [JsonPropertyName("op")] public string Op { get; set; } = null!;

    [JsonPropertyName("path")]
    public string Path { get; set; } = null!;

    [JsonPropertyName("value")]
    public object Value { get; set; } = null!;
}
