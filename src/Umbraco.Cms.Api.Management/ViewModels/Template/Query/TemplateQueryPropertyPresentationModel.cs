namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the data structure used to present a property in a template query within the management API.
/// </summary>
public class TemplateQueryPropertyPresentationModel
{
    /// <summary>
    /// Gets or sets the alias of the template query property.
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// Gets or sets the data type of the template query property.
    /// </summary>
    public required TemplateQueryPropertyType Type { get; init; }
}
