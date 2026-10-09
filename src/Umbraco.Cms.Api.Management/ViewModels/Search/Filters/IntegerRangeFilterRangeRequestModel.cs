namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A single range for an integer range filter.
/// </summary>
public class IntegerRangeFilterRangeRequestModel
{
    /// <summary>
    /// Gets or sets the inclusive lower bound, or null for no lower bound.
    /// </summary>
    public int? MinValue { get; set; }

    /// <summary>
    /// Gets or sets the exclusive upper bound, or null for no upper bound.
    /// </summary>
    public int? MaxValue { get; set; }
}
