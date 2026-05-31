using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for dictionary items in the Umbraco CMS Management API.
/// </summary>
public class DictionaryPresentationFactory : IDictionaryPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILanguageService _languageService;

    public DictionaryPresentationFactory(IUmbracoMapper umbracoMapper, ILanguageService languageService)
    {
        _umbracoMapper = umbracoMapper;
        _languageService = languageService;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="Umbraco.Cms.Api.Management.Models.DictionaryItemResponseModel" /> view model from the specified <see cref="Umbraco.Cms.Core.Models.IDictionaryItem" />.
    /// Only translations with valid language ISO codes are included in the resulting model.
    /// </summary>
    /// <param name="dictionaryItem">The dictionary item to convert into a view model.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the created <see cref="Umbraco.Cms.Api.Management.Models.DictionaryItemResponseModel" /> with filtered translations.</returns>
    public async Task<DictionaryItemResponseModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem)
    {
        DictionaryItemResponseModel dictionaryResponseModel = _umbracoMapper.Map<DictionaryItemResponseModel>(dictionaryItem)!;

        var validLanguageIsoCodes = (await _languageService.GetAllIsoCodesAsync())
            .ToArray();
        IDictionaryTranslation[] validTranslations = dictionaryItem.Translations
            .Where(t => validLanguageIsoCodes.Contains(t.LanguageIsoCode))
            .ToArray();
        dictionaryResponseModel.Translations = validTranslations
            .Select(translation => _umbracoMapper.Map<DictionaryItemTranslationModel>(translation))
            .WhereNotNull()
            .ToArray();

        return dictionaryResponseModel;
    }

    /// <summary>
    /// Asynchronously updates an existing dictionary item using values from the provided update model.
    /// </summary>
    /// <param name="current">The existing <see cref="IDictionaryItem"/> to be updated.</param>
    /// <param name="updateDictionaryItemRequestModel">The model containing updated values and translations for the dictionary item.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="IDictionaryItem"/>.</returns>
    public async Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map(updateDictionaryItemRequestModel, current);

        await MapTranslations(updated, updateDictionaryItemRequestModel.Translations);

        return updated;
    }

    /// <summary>
    /// Asynchronously maps a <see cref="CreateDictionaryItemRequestModel"/> to an <see cref="IDictionaryItem"/>, including mapping its translations.
    /// </summary>
    /// <param name="createDictionaryItemUpdateModel">The request model containing data for the new dictionary item, including translations.</param>
    /// <returns>A task representing the asynchronous operation, with the mapped <see cref="IDictionaryItem"/> as its result.</returns>
    public async Task<IDictionaryItem> MapCreateModelToDictionaryItemAsync(CreateDictionaryItemRequestModel createDictionaryItemUpdateModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map<IDictionaryItem>(createDictionaryItemUpdateModel)!;

        await MapTranslations(updated, createDictionaryItemUpdateModel.Translations);

        return updated;
    }

    public UploadDictionaryResponseModel CreateDictionaryImportViewModel(UdtFileUpload udtFileUpload) =>
        new()
        {
            FileName = udtFileUpload.FileName,
            DictionaryItems = udtFileUpload
                .Content
                .Descendants("DictionaryItem")
                .Select(dictionaryItem =>
                {
                    if (Guid.TryParse(dictionaryItem.Attributes("Key").FirstOrDefault()?.Value, out Guid itemKey) == false)
                    {
                        return null;
                    }

                    var name = dictionaryItem.Attributes("Name").FirstOrDefault()?.Value;
                    if (name.IsNullOrWhiteSpace())
                    {
                        return null;
                    }

                    Guid? parentKey = Guid.TryParse(dictionaryItem.Parent?.Attributes("Key").FirstOrDefault()?.Value, out Guid key)
                        ? key
                        : null;

                    return new ImportDictionaryItemsPresentationModel
                    {
                        Name = name,
                        Id = itemKey,
                        Parent = ReferenceByIdModel.ReferenceOrNull(parentKey)
                    };
                })
                .WhereNotNull()
                .ToArray(),
        };

    private async Task MapTranslations(IDictionaryItem dictionaryItem, IEnumerable<DictionaryItemTranslationModel> translationModels)
    {
        var languagesByIsoCode = (await _languageService.GetAllAsync()).ToDictionary(l => l.IsoCode);
        DictionaryItemTranslationModel[] validTranslations = translationModels
            .Where(translation => languagesByIsoCode.ContainsKey(translation.IsoCode))
            .ToArray();

        foreach (DictionaryItemTranslationModel translationModel in validTranslations)
        {
            dictionaryItem.AddOrUpdateDictionaryValue(languagesByIsoCode[translationModel.IsoCode], translationModel.Translation);
        }
    }
}
