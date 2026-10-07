namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A bucket of a keyword facet result.
/// </summary>
public class KeywordFacetValueResponseModel : IFacetValueResponseModel
{
    /// <summary>
    /// Gets or sets the keyword value this bucket represents.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    public required long Count { get; set; }
}
