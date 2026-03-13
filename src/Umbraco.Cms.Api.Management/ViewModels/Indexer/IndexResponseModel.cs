using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Indexer;

/// <summary>
/// Represents the response model returned after performing a search index operation.
/// </summary>
public class IndexResponseModel
{
    /// <summary>
    /// Gets the name of the indexer.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets or sets the health status of the indexer.
    /// </summary>
    public HealthStatusResponseModel HealthStatus { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether the index can be rebuilt.
    /// </summary>
    [Required]
    public bool CanRebuild { get; init; }

    /// <summary>
    /// Gets the name of the searcher associated with this index response.
    /// </summary>
    public string SearcherName { get; init; } = null!;

    /// <summary>
    /// Gets the count of documents indexed.
    /// </summary>
    [Required]
    public long DocumentCount { get; init; }

    /// <summary>
    /// Gets or sets the count of fields indexed.
    /// </summary>
    [Required]
    public int FieldCount { get; init; }

    /// <summary>
    /// Gets a collection of provider-specific properties associated with the indexer.
    /// The properties are represented as key-value pairs.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? ProviderProperties { get; init; }
}
