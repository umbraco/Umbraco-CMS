namespace Umbraco.Cms.Api.Management.ViewModels.Template;

/// <summary>
/// Represents a model for the data required to create a new template via the API.
/// </summary>
public class CreateTemplateRequestModel : TemplateModelBase
{
    /// <summary>
    /// Gets or sets the optional unique identifier of the template. This may be provided when creating a template with a specific ID.
    /// </summary>
    public Guid? Id { get; set; }
}
