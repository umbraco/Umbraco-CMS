using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.ManagementApi.ViewModels.Language;

public class LanguageViewModel
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string IsoCode { get; set; } = null!;

    public string? Name { get; set; }

    public bool IsDefault { get; set; }

    public bool IsMandatory { get; set; }

    public int? FallbackLanguageId { get; set; }
}
