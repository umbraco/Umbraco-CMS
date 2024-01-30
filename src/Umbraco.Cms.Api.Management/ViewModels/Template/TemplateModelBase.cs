using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Alias { get; set; } = string.Empty;

    public string? Content { get; set; }
}
