namespace Umbraco.Cms.ManagementApi.ViewModels.Search;

public class IndexViewModel
{
    public string Name { get; init; } = null!;

    public string? HealthStatus { get; init; }

    public bool IsHealthy => HealthStatus == "Healthy";

    public bool CanRebuild { get; init; }

    public string SearcherName { get; init; } = null!;

    public long DocumentCount { get; init; }

    public int FieldCount { get; init; }
}
