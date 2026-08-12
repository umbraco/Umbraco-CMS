namespace Umbraco.Cms.Search.Core.Models.ViewModels;

/// <summary>
/// The Management API representation of a page of search results.
/// </summary>
public class SearchResultViewModel
{
    /// <summary>
    /// Gets or sets the total number of matching documents, regardless of paging.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the matched documents for the requested page.
    /// </summary>
    public required IEnumerable<DocumentViewModel> Documents { get; set; }

    /// <summary>
    /// Gets or sets the facet results for the requested facets, if any.
    /// </summary>
    public required IEnumerable<FacetResultViewModel> Facets { get; set; }
}
