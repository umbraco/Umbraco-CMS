namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A filter that matches documents whose integer field equals any of the given values.
/// </summary>
public class IntegerExactFilterRequestModel : IFilterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the integer values to match.
    /// </summary>
    public required int[] Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    public required bool Negate { get; set; }
}
