using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;

namespace Umbraco.Cms.ManagementApi.Mapping.Dictionary;

public class DictionaryViewModelMapDefinition : IMapDefinition
{
    private readonly ILocalizationService _localizationService;
    private readonly CommonMapper _commonMapper;
    private readonly IDictionaryService _dictionaryService;

    public DictionaryViewModelMapDefinition(ILocalizationService localizationService, CommonMapper commonMapper, IDictionaryService dictionaryService)
    {
        _localizationService = localizationService;
        _commonMapper = commonMapper;
        _dictionaryService = dictionaryService;
    }
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<DictionaryViewModel, IDictionaryItem>((source, context) => new DictionaryItem(string.Empty), Map);
        mapper.Define<IDictionaryItem, DictionaryViewModel>((source, context) => new DictionaryViewModel(), Map);
        mapper.Define<DictionaryTranslationViewModel, IDictionaryTranslation>((source, context) => new DictionaryTranslation(source.LanguageId, string.Empty), Map);
        mapper.Define<IDictionaryItem, DictionaryOverviewViewModel>((source, context) => new DictionaryOverviewViewModel(), Map);

    }

    // Umbraco.Code.MapAll -Id
    private void Map(DictionaryViewModel source, IDictionaryItem target, MapperContext context)
    {
        target.CreateDate = source.CreateDate;
        target.ItemKey = source.Name!;
        target.Key = source.Key;
        target.ParentId = source.ParentId;
        target.Translations = context.MapEnumerable<DictionaryTranslationViewModel, IDictionaryTranslation>(source.Translations);
        target.UpdateDate = source.UpdateDate;
        target.DeleteDate = null;

    }

    // Umbraco.Code.MapAll -CreateDate -DeleteDate -UpdateDate -Language
    private void Map(DictionaryTranslationViewModel source, IDictionaryTranslation target, MapperContext context)
    {
        target.Value = source.Translation;
        target.Id = source.Id;
        target.Key = source.Key;
    }

    // Umbraco.Code.MapAll -Icon -Trashed -Alias -NameIsDirty -ContentApps -Path -Translations
    private void Map(IDictionaryItem source, DictionaryViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;
        target.ParentId = source.ParentId ?? null;
        target.CreateDate = source.CreateDate;
        target.UpdateDate = source.UpdateDate;
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
