using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class SearchResultViewModel
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("fieldCount")]
    public int FieldCount => Fields?.Count() ?? 0;

    [JsonPropertyName("values")]
    public IEnumerable<FieldViewModel>? Fields { get; set; }
}
