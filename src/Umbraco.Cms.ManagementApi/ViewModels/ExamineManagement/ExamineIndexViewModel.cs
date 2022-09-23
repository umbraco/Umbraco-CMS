namespace Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

public class ExamineIndexViewModel
{
    public string? Name { get; set; }

    public string? HealthStatus { get; set; }

    public bool IsHealthy => HealthStatus == "Healthy";

    public IReadOnlyDictionary<string, object?> ProviderProperties { get; set; } = null!;

    public bool CanRebuild { get; set; }

    public string? SearcherName { get; set; }
}
