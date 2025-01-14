using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDictionaryPresentationFactory
{
    Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel);

    Task<IDictionaryItem> MapCreateModelToDictionaryItemAsync(CreateDictionaryItemRequestModel createDictionaryItemUpdateModel);

    Task<DictionaryItemResponseModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem);

    UploadDictionaryResponseModel CreateDictionaryImportViewModel(UdtFileUpload fileUpload);
}
