using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDictionaryFactory
{
    IDictionaryItem MapUpdateModelToDictionaryItem(Guid key, DictionaryItemUpdateModel dictionaryItemUpdateModel);

    IEnumerable<IDictionaryTranslation> MapTranslations(IEnumerable<DictionaryItemTranslationModel> translationModels);

    DictionaryItemViewModel CreateDictionaryItemViewModel(IDictionaryItem dictionaryItem);

    DictionaryImportViewModel CreateDictionaryImportViewModel(FormFileUploadResult formFileUploadResult);
}
