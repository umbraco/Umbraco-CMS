using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

/// <summary>
/// Serves as the base view model for templates in the Umbraco CMS Management API.
/// </summary>
public class TemplateModelBase
{
    /// <summary>
    /// Gets or sets the name of the template.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias of the template.
    /// </summary>
    [Required]
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the markup or code content of the template.
    /// </summary>
    public string? Content { get; set; }
}
