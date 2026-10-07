namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A bucket of a date/time exact facet result.
/// </summary>
public class DateTimeOffsetExactFacetValueResponseModel : IFacetValueResponseModel
{
    /// <summary>
    /// Gets or sets the date/time value this bucket represents.
    /// </summary>
    public required DateTimeOffset Key { get; set; }

    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    public required long Count { get; set; }
}
