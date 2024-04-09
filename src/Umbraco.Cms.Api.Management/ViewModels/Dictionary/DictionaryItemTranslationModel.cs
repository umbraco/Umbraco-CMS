using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryItemTranslationModel
{
    [Required]
    public string IsoCode { get; set; } = string.Empty;

    [Required]
    public string Translation { get; set; } = string.Empty;
}
