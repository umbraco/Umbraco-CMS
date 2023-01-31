namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

public class DictionaryItemModelBase
{
    public string Name { get; set; } = null!;

    public IEnumerable<DictionaryItemTranslationModel> Translations { get; set; } = Array.Empty<DictionaryItemTranslationModel>();
}
