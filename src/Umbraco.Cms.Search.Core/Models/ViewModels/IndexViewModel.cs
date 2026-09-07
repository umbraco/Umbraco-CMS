using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

/// <summary>
/// The Management API representation of a search index's metadata.
/// </summary>
public class IndexViewModel
{
    /// <summary>
    /// Gets or sets the alias of the index.
    /// </summary>
    public required string IndexAlias { get; set; }

    /// <summary>
    /// Gets or sets the name of the search provider backing the index.
    /// </summary>
    public required string ProviderName { get; set; }

    /// <summary>
    /// Gets or sets the number of documents in the index.
    /// </summary>
    public long DocumentCount { get; set; }

    /// <summary>
    /// Gets or sets the health of the index.
    /// </summary>
    public HealthStatus HealthStatus { get; set; }
}
