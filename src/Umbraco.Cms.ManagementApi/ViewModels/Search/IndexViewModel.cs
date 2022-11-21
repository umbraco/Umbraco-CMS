using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Search;

public class IndexViewModel
{
    [Required]
    public string Name { get; init; } = null!;

    public string? HealthStatus { get; init; }

    [Required]
    public bool IsHealthy => HealthStatus == "Healthy";

    [Required]
    public bool CanRebuild { get; init; }

    public string SearcherName { get; init; } = null!;

    [Required]
    public long DocumentCount { get; init; }

    [Required]
    public int FieldCount { get; init; }

    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; init; }
}
