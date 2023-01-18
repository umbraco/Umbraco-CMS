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
    private readonly ILocalizationService _localizationService;

    public DictionaryFactory(IUmbracoMapper umbracoMapper, ILocalizationService localizationService)
    {
        _umbracoMapper = umbracoMapper;
        _localizationService = localizationService;
    }

    public DictionaryItemViewModel CreateDictionaryItemViewModel(IDictionaryItem dictionaryItem)
    {
        DictionaryItemViewModel dictionaryViewModel = _umbracoMapper.Map<DictionaryItemViewModel>(dictionaryItem)!;

        var validLanguageIds = _localizationService
            .GetAllLanguages()
            .Select(language => language.Id)
            .ToArray();
        IDictionaryTranslation[] validTranslations = dictionaryItem.Translations
            .Where(t => validLanguageIds.Contains(t.LanguageId))
            .ToArray();
        dictionaryViewModel.Translations = validTranslations
            .Select(translation => _umbracoMapper.Map<DictionaryItemTranslationModel>(translation))
            .WhereNotNull()
            .ToArray();

        return dictionaryViewModel;
    }

    public IDictionaryItem MapUpdateModelToDictionaryItem(IDictionaryItem current, DictionaryItemUpdateModel dictionaryItemUpdateModel)
    {
        IDictionaryItem updated = _umbracoMapper.Map(dictionaryItemUpdateModel, current);

        MapTranslations(updated, dictionaryItemUpdateModel.Translations);

        return updated;
    }

    public IEnumerable<IDictionaryTranslation> MapTranslations(IEnumerable<DictionaryItemTranslationModel> translationModels)
    {
        var temporaryDictionaryItem = new DictionaryItem(Guid.NewGuid().ToString());

        MapTranslations(temporaryDictionaryItem, translationModels);

        return temporaryDictionaryItem.Translations;
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

    private void MapTranslations(IDictionaryItem dictionaryItem, IEnumerable<DictionaryItemTranslationModel> translationModels)
    {
        var languagesByIsoCode = _localizationService
            .GetAllLanguages()
            .ToDictionary(l => l.IsoCode);
        DictionaryItemTranslationModel[] validTranslations = translationModels
            .Where(translation => languagesByIsoCode.ContainsKey(translation.IsoCode))
            .ToArray();

        foreach (DictionaryItemTranslationModel translationModel in validTranslations)
        {
            _localizationService.AddOrUpdateDictionaryValue(dictionaryItem, languagesByIsoCode[translationModel.IsoCode], translationModel.Translation);
        }
    }
}
