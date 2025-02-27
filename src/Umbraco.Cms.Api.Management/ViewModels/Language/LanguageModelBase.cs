using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language;

public class LanguageModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsMandatory { get; set; }

    public string? FallbackIsoCode { get; set; }
}
