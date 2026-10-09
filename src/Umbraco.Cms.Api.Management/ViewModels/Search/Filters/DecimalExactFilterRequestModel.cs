namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A filter that matches documents whose decimal field equals any of the given values.
/// </summary>
public class DecimalExactFilterRequestModel : IFilterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the decimal values to match.
    /// </summary>
    public required decimal[] Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    public required bool Negate { get; set; }
}
