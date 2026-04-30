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
    /// Gets or sets a reference to the parent (layout) template, if any.
    /// </summary>
    public ReferenceByIdModel? LayoutTemplate { get; set; }

    [Obsolete("Use LayoutTemplate instead. Scheduled for removal in Umbraco 20.")]
    public ReferenceByIdModel? MasterTemplate { get => LayoutTemplate; set => LayoutTemplate = value; }
}
