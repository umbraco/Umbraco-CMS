namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents the source node for a template query.
/// </summary>
public class SourceModel
{
    /// <summary>
    ///     Gets or sets the unique identifier of the source node.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the source node.
    /// </summary>
    public string? Name { get; set; }
}
