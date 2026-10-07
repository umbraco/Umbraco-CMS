namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A facet that counts matching documents per date/time range.
/// </summary>
public class DateTimeOffsetRangeFacetRequestModel : IFacetRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to facet on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the ranges to count matching documents for.
    /// </summary>
    public required DateTimeOffsetRangeFacetRangeRequestModel[] Ranges { get; set; }
}
