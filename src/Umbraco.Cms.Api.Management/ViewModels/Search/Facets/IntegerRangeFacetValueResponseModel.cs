namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A bucket of an integer range facet result.
/// </summary>
public class IntegerRangeFacetValueResponseModel : IFacetValueResponseModel
{
    /// <summary>
    /// Gets or sets the key of the requested range this bucket represents.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the lower bound of the range, if any.
    /// </summary>
    public int? Min { get; set; }

    /// <summary>
    /// Gets or sets the upper bound of the range, if any.
    /// </summary>
    public int? Max { get; set; }

    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    public required long Count { get; set; }
}
