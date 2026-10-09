namespace Umbraco.Cms.Api.Management.ViewModels.Search;

/// <summary>
/// The Management API representation of a page of search results.
/// </summary>
public class SearchResultResponseModel
{
    /// <summary>
    /// Gets or sets the total number of matching documents, regardless of paging.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Gets or sets the matched documents for the requested page.
    /// </summary>
    public required IEnumerable<SearchDocumentResponseModel> Documents { get; set; }

    /// <summary>
    /// Gets or sets the facet results for the requested facets, if any.
    /// </summary>
    public required IEnumerable<FacetResultResponseModel> Facets { get; set; }
}
