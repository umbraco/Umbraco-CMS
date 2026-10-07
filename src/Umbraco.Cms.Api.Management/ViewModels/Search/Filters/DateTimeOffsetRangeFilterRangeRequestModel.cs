namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A single range for a date/time range filter.
/// </summary>
public class DateTimeOffsetRangeFilterRangeRequestModel
{
    /// <summary>
    /// Gets or sets the inclusive lower bound, or null for no lower bound.
    /// </summary>
    public DateTimeOffset? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the exclusive upper bound, or null for no upper bound.
    /// </summary>
    public DateTimeOffset? MaxValue { get; set; }
}
