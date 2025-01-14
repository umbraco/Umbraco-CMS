using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryItemModelBase
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public IEnumerable<DictionaryItemTranslationModel> Translations { get; set; } = Enumerable.Empty<DictionaryItemTranslationModel>();
}
