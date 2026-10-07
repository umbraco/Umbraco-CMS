namespace Umbraco.Cms.Api.Management.ViewModels.Search.Filters;

/// <summary>
/// A filter that matches documents whose full-text field contains any of the given values.
/// </summary>
public class TextFilterRequestModel : IFilterRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to filter on.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the values to search for within the text of the field.
    /// </summary>
    public required string[] Values { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to match documents that do NOT satisfy the filter.
    /// </summary>
    public required bool Negate { get; set; }
}
