using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Language.Item;

public class LanguageItemResponseModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string IsoCode { get; set; } = string.Empty;
}
