using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Api.Management.Mapping.Dictionary;

public class DictionaryMapDefinition : IMapDefinition
{
    private readonly ILocalizationService _localizationService;

    public DictionaryMapDefinition(ILocalizationService localizationService) => _localizationService = localizationService;

    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IDictionaryItem, DictionaryItemViewModel>((_, _) => new DictionaryItemViewModel(), Map);
        mapper.Define<IDictionaryTranslation, DictionaryItemTranslationModel>((_, _) => new DictionaryItemTranslationModel(), Map);
        mapper.Define<DictionaryItemUpdateModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<DictionaryItemCreateModel, IDictionaryItem>((_, _) => new DictionaryItem(string.Empty), Map);
        mapper.Define<IDictionaryItem, DictionaryOverviewViewModel>((_, _) => new DictionaryOverviewViewModel(), Map);
    }

    // Umbraco.Code.MapAll -Translations
    private void Map(IDictionaryItem source, DictionaryItemViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;
    }

    // Umbraco.Code.MapAll
    private void Map(IDictionaryTranslation source, DictionaryItemTranslationModel target, MapperContext context)
    {
        target.IsoCode = source.Language?.IsoCode ?? throw new ArgumentException("Translation has no language", nameof(source));
        target.Translation = source.Value;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -ParentId -Translations
    private void Map(DictionaryItemUpdateModel source, IDictionaryItem target, MapperContext context)
    {
        target.ItemKey = source.Name;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll -Id -Key -CreateDate -UpdateDate -Translations
    private void Map(DictionaryItemCreateModel source, IDictionaryItem target, MapperContext context)
    {
        target.ItemKey = source.Name;
        target.ParentId = source.ParentKey;
        target.DeleteDate = null;
    }

    // Umbraco.Code.MapAll -Level -Translations
    private void Map(IDictionaryItem source, DictionaryOverviewViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;

        // add all languages and  the translations
        foreach (ILanguage lang in _localizationService.GetAllLanguages())
        {
            var langId = lang.Id;
            IDictionaryTranslation? translation = source.Translations?.FirstOrDefault(x => x.LanguageId == langId);

            target.Translations.Add(
                new DictionaryTranslationOverviewViewModel
                {
                    DisplayName = lang.CultureName,
                    HasTranslation = translation != null && string.IsNullOrEmpty(translation.Value) == false,
                });
        }
    }
}
