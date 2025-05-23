using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DictionaryPresentationFactory : IDictionaryPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILanguageService _languageService;

    public DictionaryPresentationFactory(IUmbracoMapper umbracoMapper, ILanguageService languageService)
    {
        _umbracoMapper = umbracoMapper;
        _languageService = languageService;
    }

    public async Task<DictionaryItemResponseModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem)
    {
        DictionaryItemResponseModel dictionaryResponseModel = _umbracoMapper.Map<DictionaryItemResponseModel>(dictionaryItem)!;

        var validLanguageIsoCodes = (await _languageService.GetAllAsync())
            .Select(language => language.IsoCode)
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

    public async Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, UpdateDictionaryItemRequestModel updateDictionaryItemRequestModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map(updateDictionaryItemRequestModel, current);

        await MapTranslations(updated, updateDictionaryItemRequestModel.Translations);

        return updated;
    }

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
