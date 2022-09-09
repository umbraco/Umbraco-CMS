using System.Text.Json.Serialization;

namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class ExamineIndexViewModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("healthStatus")]
    public string? HealthStatus { get; set; }

    [JsonPropertyName("isHealthy")]
    public bool IsHealthy => HealthStatus == "Healthy";

    [JsonPropertyName("providerProperties")]
    public IReadOnlyDictionary<string, object?> ProviderProperties { get; set; } = null!;

    [JsonPropertyName("canRebuild")]
    public bool CanRebuild { get; set; }
}
