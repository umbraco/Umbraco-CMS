namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A single range for a decimal range filter.
/// </summary>
public class DecimalRangeFilterRangeRequestModel
{
    /// <summary>
    /// Gets or sets the inclusive lower bound, or null for no lower bound.
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the exclusive upper bound, or null for no upper bound.
    /// </summary>
    public decimal? MaxValue { get; set; }
}
