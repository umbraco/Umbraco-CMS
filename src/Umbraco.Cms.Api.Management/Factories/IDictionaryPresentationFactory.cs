using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for dictionary items.
/// </summary>
public interface IDictionaryPresentationFactory
{
    /// <summary>
    /// Maps the update model to an existing dictionary item asynchronously.
    /// </summary>
    /// <param name="current">The current dictionary item to update.</param>
    /// <param name="updateDictionaryItemRequestModel">The update model containing new values for the dictionary item.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated dictionary item.</returns>
    Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel);

    /// <summary>
    /// Asynchronously maps a <see cref="CreateDictionaryItemRequestModel"/> to an <see cref="IDictionaryItem"/>.
    /// </summary>
    /// <param name="createDictionaryItemUpdateModel">The model containing data for the new dictionary item.</param>
    /// <returns>A task representing the asynchronous operation, with the mapped <see cref="IDictionaryItem"/> as its result.</returns>
    Task<IDictionaryItem> MapCreateModelToDictionaryItemAsync(CreateDictionaryItemRequestModel createDictionaryItemUpdateModel);

    /// <summary>
    /// Creates a view model representation of a dictionary item asynchronously.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to create the view model from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the dictionary item response model.</returns>
    Task<DictionaryItemResponseModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem);

    /// <summary>
    /// Creates a response model for importing dictionary items from the specified uploaded file.
    /// </summary>
    /// <param name="fileUpload">The uploaded file containing the dictionary items to import.</param>
    /// <returns>A model representing the result of the dictionary import operation.</returns>
    UploadDictionaryResponseModel CreateDictionaryImportViewModel(UdtFileUpload fileUpload);
}
