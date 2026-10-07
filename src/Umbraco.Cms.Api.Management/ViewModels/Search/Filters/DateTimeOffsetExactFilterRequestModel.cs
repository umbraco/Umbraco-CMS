namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A filter that matches documents whose date/time field equals any of the given values.
/// </summary>
public class DateTimeOffsetExactFilterRequestModel : IFilterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the date/time values to match.
    /// </summary>
    public required DateTimeOffset[] Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    public required bool Negate { get; set; }
}
