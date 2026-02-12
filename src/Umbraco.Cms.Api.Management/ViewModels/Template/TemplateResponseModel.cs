namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateResponseModel : TemplateModelBase
{
    public Guid Id { get; set; }

    public ReferenceByIdModel? LayoutTemplate { get; set; }

    [Obsolete("Use LayoutTemplate instead. Scheduled for removal in Umbraco 20.")]
    public ReferenceByIdModel? MasterTemplate { get => LayoutTemplate; set => LayoutTemplate = value; }
}
