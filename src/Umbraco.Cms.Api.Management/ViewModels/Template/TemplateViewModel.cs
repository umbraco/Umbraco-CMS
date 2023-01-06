namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateViewModel : TemplateModelBase
{
    public Guid Key { get; set; }

    public bool IsMasterTemplate { get; set; }

    public string? VirtualPath { get; set; }
}
