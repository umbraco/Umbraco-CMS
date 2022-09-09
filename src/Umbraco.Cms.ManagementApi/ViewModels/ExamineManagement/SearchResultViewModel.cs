using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class SearchResultViewModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("fieldCount")]
    public int FieldCount => Values?.Count ?? 0;

    [JsonPropertyName("values")]
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Values { get; set; }
}
