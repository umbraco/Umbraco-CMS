namespace Umbraco.Cms.Search.Provider.Examine.Models.ViewModels;

/// <summary>
/// One indexed field's raw name, type, and values, as returned by the backoffice "Show Fields" feature.
/// </summary>
public class FieldViewModel
{
    /// <summary>
    /// Gets or sets the raw indexed field name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the field's Lucene type, if known.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the field's indexed values.
    /// </summary>
    public required IReadOnlyCollection<string> Values { get; set; }
}
