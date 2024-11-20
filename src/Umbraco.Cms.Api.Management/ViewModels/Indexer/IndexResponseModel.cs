using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Indexer;

public class IndexResponseModel
{
    [Required]
    public string Name { get; init; } = null!;

    public HealthStatusResponseModel HealthStatus { get; set; } = new();

    [Required]
    public bool CanRebuild { get; init; }

    public string SearcherName { get; init; } = null!;

    [Required]
    public long DocumentCount { get; init; }

    [Required]
    public int FieldCount { get; init; }

    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; init; }
}
