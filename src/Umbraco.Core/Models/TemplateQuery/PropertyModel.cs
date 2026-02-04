namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents a property that can be used in template queries for filtering or sorting.
/// </summary>
public class PropertyModel
{
    /// <summary>
    ///     Gets or sets the display name of the property.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the alias of the property.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the data type of the property (e.g., "string", "datetime", "boolean").
    /// </summary>
    public string? Type { get; set; }
}
