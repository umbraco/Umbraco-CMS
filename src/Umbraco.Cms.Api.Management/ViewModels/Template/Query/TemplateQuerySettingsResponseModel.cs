namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the model returned in the API response for template query settings.
/// </summary>
public class TemplateQuerySettingsResponseModel
{
    /// <summary>
    /// Gets or sets the aliases of document types associated with the template query settings.
    /// </summary>
    public required IEnumerable<string> DocumentTypeAliases { get; init; }

    /// <summary>
    /// Gets or sets the collection of properties available for template queries.
    /// Each property is represented by a <see cref="TemplateQueryPropertyPresentationModel"/>.
    /// </summary>
    public required IEnumerable<TemplateQueryPropertyPresentationModel> Properties { get; init; }

    /// <summary>
    /// Gets or sets the collection of template query operators.
    /// </summary>
    public required IEnumerable<TemplateQueryOperatorViewModel> Operators { get; init; }
}
