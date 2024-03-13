using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language;

public class LanguageResponseModel : LanguageModelBase
{
    [Required]
    public string IsoCode { get; set; } = string.Empty;
}
