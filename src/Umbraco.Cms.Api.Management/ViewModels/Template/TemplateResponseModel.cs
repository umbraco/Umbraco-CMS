namespace Umbraco.Cms.Api.Management.ViewModels.Template;

/// <summary>
/// Represents a template response model in the Umbraco CMS Management API.
/// </summary>
public class TemplateResponseModel : TemplateModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the template.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent (master) template, if any.
    /// </summary>
    public ReferenceByIdModel? MasterTemplate { get; set; }
}
