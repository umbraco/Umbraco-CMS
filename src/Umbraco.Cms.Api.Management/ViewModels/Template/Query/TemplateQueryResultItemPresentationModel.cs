namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the data returned for an individual item in a template query result.
/// </summary>
public class TemplateQueryResultItemPresentationModel
{
    /// <summary>
    /// Gets or sets the icon associated with the template query result item.
    /// </summary>
    public required string Icon { get; init; }

    /// <summary>
    /// Gets or sets the name of the template.
    /// </summary>
    public required string Name { get; init; }
}
