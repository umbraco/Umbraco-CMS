namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateScaffoldResponseModel
{
    public string Content { get; set; } = string.Empty;

    public string Type => Constants.ResourceObjectTypes.TemplateScaffold;
}
