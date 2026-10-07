namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A filter that matches documents whose integer field falls within any of the given ranges.
/// </summary>
public class IntegerRangeFilterRequestModel : IFilterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the ranges to match.
    /// </summary>
    public required IntegerRangeFilterRangeRequestModel[] Ranges { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    public required bool Negate { get; set; }
}
