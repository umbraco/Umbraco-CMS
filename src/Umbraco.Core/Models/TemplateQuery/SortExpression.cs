namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents a sort expression for ordering query results.
/// </summary>
public class SortExpression
{
    /// <summary>
    ///     Gets or sets the property to sort by.
    /// </summary>
    public PropertyModel? Property { get; set; }

    /// <summary>
    ///     Gets or sets the sort direction (e.g., "ascending" or "descending").
    /// </summary>
    public string? Direction { get; set; }
}
