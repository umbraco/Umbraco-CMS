using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateScaffoldResponseModel
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
