namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A bucket of an integer exact facet result.
/// </summary>
public class IntegerExactFacetValueResponseModel : IFacetValueResponseModel
{
    /// <summary>
    /// Gets or sets the integer value this bucket represents.
    /// </summary>
    public required int Key { get; set; }

    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    public required long Count { get; set; }
}
