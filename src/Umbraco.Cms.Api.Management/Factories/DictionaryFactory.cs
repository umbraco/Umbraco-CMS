using System.Xml;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Models;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DictionaryFactory : IDictionaryFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILanguageService _languageService;

    public DictionaryFactory(IUmbracoMapper umbracoMapper, ILanguageService languageService)
    {
        _umbracoMapper = umbracoMapper;
        _languageService = languageService;
    }

    public async Task<DictionaryItemViewModel> CreateDictionaryItemViewModelAsync(IDictionaryItem dictionaryItem)
    {
        DictionaryItemViewModel dictionaryViewModel = _umbracoMapper.Map<DictionaryItemViewModel>(dictionaryItem)!;

        var validLanguageIsoCodes = (await _languageService.GetAllAsync())
            .Select(language => language.IsoCode)
            .ToArray();
        IDictionaryTranslation[] validTranslations = dictionaryItem.Translations
            .Where(t => validLanguageIsoCodes.Contains(t.IsoCode))
            .ToArray();
        dictionaryViewModel.Translations = validTranslations
            .Select(translation => _umbracoMapper.Map<DictionaryItemTranslationModel>(translation))
            .WhereNotNull()
            .ToArray();

        return dictionaryViewModel;
    }

    public async Task<IDictionaryItem> MapUpdateModelToDictionaryItemAsync(IDictionaryItem current, DictionaryItemUpdateModel dictionaryItemUpdateModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map(dictionaryItemUpdateModel, current);

        await MapTranslations(updated, dictionaryItemUpdateModel.Translations);

        return updated;
    }

    public async Task<IDictionaryItem> MapCreateModelToDictionaryItemAsync(DictionaryItemCreateModel dictionaryItemUpdateModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map<IDictionaryItem>(dictionaryItemUpdateModel)!;

        await MapTranslations(updated, dictionaryItemUpdateModel.Translations);

        return updated;
    }

    public DictionaryImportViewModel CreateDictionaryImportViewModel(FormFileUploadResult formFileUploadResult)
    {
        if (formFileUploadResult.CouldLoad is false || formFileUploadResult.XmlDocument is null)
        {
            throw new ArgumentNullException("The document of the FormFileUploadResult cannot be null");
        }

        var model = new DictionaryImportViewModel
        {
            TempFileName = formFileUploadResult.TemporaryPath, DictionaryItems = new List<DictionaryItemsImportViewModel>(),
        };

        var level = 1;
        var currentParent = string.Empty;
        foreach (XmlNode dictionaryItem in formFileUploadResult.XmlDocument.GetElementsByTagName("DictionaryItem"))
        {
            var name = dictionaryItem.Attributes?.GetNamedItem("Name")?.Value ?? string.Empty;
            var parentKey = dictionaryItem.ParentNode?.Attributes?.GetNamedItem("Key")?.Value ?? string.Empty;

            if (parentKey != currentParent || level == 1)
            {
                level += 1;
                currentParent = parentKey;
            }

            model.DictionaryItems.Add(new DictionaryItemsImportViewModel { Level = level, Name = name });
        }

        return model;
    }

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
