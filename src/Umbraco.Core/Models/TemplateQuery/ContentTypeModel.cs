namespace Umbraco.Cms.Core.Models.TemplateQuery;

/// <summary>
///     Represents a content type model for use in template queries.
/// </summary>
public class ContentTypeModel
{
    /// <summary>
    ///     Gets or sets the alias of the content type.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the display name of the content type.
    /// </summary>
    public string? Name { get; set; }
}
