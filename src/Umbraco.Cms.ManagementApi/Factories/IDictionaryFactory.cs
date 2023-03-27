using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.New.Cms.Core.Factories;

public interface IDictionaryFactory
{
    IDictionaryItem CreateDictionaryItem(DictionaryViewModel dictionaryViewModel);
    DictionaryViewModel CreateDictionaryViewModel(IDictionaryItem dictionaryItem);

    DictionaryImportViewModel CreateDictionaryImportViewModel(FormFileUploadResult formFileUploadResult);
}
