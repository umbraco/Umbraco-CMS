namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A single named range for an integer range facet.
/// </summary>
public class IntegerRangeFacetRangeRequestModel
{
    /// <summary>
    /// Gets or sets the key that identifies this range in the facet result.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the inclusive lower bound, or null for no lower bound.
    /// </summary>
    public int? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the exclusive upper bound, or null for no upper bound.
    /// </summary>
    public int? MaxValue { get; set; }
}
