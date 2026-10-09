namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A bucket of a decimal exact facet result.
/// </summary>
public class DecimalExactFacetValueResponseModel : IFacetValueResponseModel
{
    /// <summary>
    /// Gets or sets the decimal value this bucket represents.
    /// </summary>
    public required decimal Key { get; set; }

    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    public required long Count { get; set; }
}
