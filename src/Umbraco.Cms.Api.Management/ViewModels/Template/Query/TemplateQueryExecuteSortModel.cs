namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the available sorting options when executing a template query in the management API.
/// </summary>
public class TemplateQueryExecuteSortModel
{
    /// <summary>
    /// Gets or sets the alias of the property to sort by.
    /// </summary>
    public string PropertyAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the direction of the sort, such as ascending or descending.
    /// </summary>
    public string? Direction { get; set; }
}
