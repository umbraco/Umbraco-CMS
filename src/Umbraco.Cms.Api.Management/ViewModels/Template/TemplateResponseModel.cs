namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateResponseModel : TemplateModelBase
{
    public Guid Id { get; set; }

    public ReferenceByIdModel? MasterTemplate { get; set; }
}
