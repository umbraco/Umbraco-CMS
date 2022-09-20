using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.ManagementApi.Factories;

public class DictionaryFactory : IDictionaryFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILocalizationService _localizationService;
    private readonly IDictionaryService _dictionaryService;
    private readonly CommonMapper _commonMapper;

    public DictionaryFactory(
        IUmbracoMapper umbracoMapper,
        ILocalizationService localizationService,
        IDictionaryService dictionaryService,
        CommonMapper commonMapper)
    {
        _umbracoMapper = umbracoMapper;
        _localizationService = localizationService;
        _dictionaryService = dictionaryService;
        _commonMapper = commonMapper;
    }

    public IDictionaryItem CreateDictionaryItem(DictionaryViewModel dictionaryViewModel)
    {
        IDictionaryItem mappedItem = _umbracoMapper.Map<IDictionaryItem>(dictionaryViewModel)!;
        IDictionaryItem? dictionaryItem = _localizationService.GetDictionaryItemById(dictionaryViewModel.Key);
        mappedItem.Id = dictionaryItem!.Id;
        return mappedItem;
    }

    public DictionaryViewModel CreateDictionaryViewModel(IDictionaryItem dictionaryItem)
    {
        DictionaryViewModel dictionaryViewModel = _umbracoMapper.Map<DictionaryViewModel>(dictionaryItem)!;

        dictionaryViewModel.ContentApps = _commonMapper.GetContentAppsForEntity(dictionaryItem);
        dictionaryViewModel.Path = _dictionaryService.CalculatePath(dictionaryItem.ParentId, dictionaryItem.Id);

        var translations = new List<DictionaryTranslationViewModel>();
        // add all languages and  the translations
        foreach (ILanguage lang in _localizationService.GetAllLanguages())
        {
            var langId = lang.Id;
            IDictionaryTranslation? translation = dictionaryItem.Translations?.FirstOrDefault(x => x.LanguageId == langId);

            translations.Add(new DictionaryTranslationViewModel
            {
                IsoCode = lang.IsoCode,
                DisplayName = lang.CultureName,
                Translation = translation?.Value ?? string.Empty,
                LanguageId = lang.Id,
                Id = translation?.Id ?? 0,
                Key = translation?.Key ?? Guid.Empty,
            });
        }

        dictionaryViewModel.Translations = translations;

        return dictionaryViewModel;
    }
}
