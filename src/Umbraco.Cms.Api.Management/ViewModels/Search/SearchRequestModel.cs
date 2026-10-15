namespace Umbraco.Cms.Api.Management.ViewModels.Search;

/// <summary>
/// The Management API request body for executing a search.
/// </summary>
public class SearchRequestModel
{
    /// <summary>
    /// Gets or sets the alias of the index to search.
    /// </summary>
    public required string IndexAlias { get; set; }

    /// <summary>
    /// Gets or sets the full-text search query.
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Gets or sets the culture to search within.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the segment to search within.
    /// </summary>
    public string? Segment { get; set; }
}
