using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDictionaryFactory
{
    IDictionaryItem MapDictionaryItemUpdate(IDictionaryItem current, DictionaryItemUpdateModel dictionaryItemUpdateModel);

    IDictionaryItem MapDictionaryItemCreate(DictionaryItemCreateModel dictionaryItemUpdateModel);

    DictionaryItemViewModel CreateDictionaryItemViewModel(IDictionaryItem dictionaryItem);

    DictionaryImportViewModel CreateDictionaryImportViewModel(FormFileUploadResult formFileUploadResult);
}
