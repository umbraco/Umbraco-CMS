namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Alias { get; set; } = string.Empty;

    public string? Content { get; set; }
}
