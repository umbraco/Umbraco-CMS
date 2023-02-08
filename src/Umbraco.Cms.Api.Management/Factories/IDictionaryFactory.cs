using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDictionaryFactory
{
    Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, DictionaryItemUpdateModel dictionaryItemUpdateModel);

    Task<IDictionaryItem> MapCreateModelToDictionaryItemAsync(DictionaryItemCreateModel dictionaryItemUpdateModel);

    Task<DictionaryItemViewModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem);

    DictionaryUploadViewModel CreateDictionaryImportViewModel(UdtFileUpload fileUpload);
}
