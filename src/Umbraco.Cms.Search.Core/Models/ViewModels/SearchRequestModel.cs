using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

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
    /// Gets or sets the filters to apply.
    /// </summary>
    public IEnumerable<Filter>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the facets to request.
    /// </summary>
    public IEnumerable<Facet>? Facets { get; set; }

    /// <summary>
    /// Gets or sets the sorters to apply.
    /// </summary>
    public IEnumerable<Sorter>? Sorters { get; set; }

    /// <summary>
    /// Gets or sets the culture to search within.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the segment to search within.
    /// </summary>
    public string? Segment { get; set; }
}
