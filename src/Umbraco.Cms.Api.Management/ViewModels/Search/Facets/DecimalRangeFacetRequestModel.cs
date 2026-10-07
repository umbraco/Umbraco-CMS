namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A facet that counts matching documents per decimal range.
/// </summary>
public class DecimalRangeFacetRequestModel : IFacetRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to facet on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the ranges to count matching documents for.
    /// </summary>
    public required DecimalRangeFacetRangeRequestModel[] Ranges { get; set; }
}
