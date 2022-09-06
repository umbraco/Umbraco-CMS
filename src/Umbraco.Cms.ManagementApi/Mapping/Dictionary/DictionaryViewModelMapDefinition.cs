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

    // Umbraco.Code.MapAll -Icon -Trashed -Alias -NameIsDirty
    private void Map(IDictionaryItem source, DictionaryViewModel target, MapperContext context)
    {
        target.Key = source.Key;
        target.Name = source.ItemKey;
        target.ParentId = source.ParentId ?? null;
        target.ContentApps = _commonMapper.GetContentAppsForEntity(source);
        target.Path = _dictionaryService.CalculatePath(source.ParentId, source.Id);

        var translations = new List<DictionaryTranslationViewModel>();
        // add all languages and  the translations
        foreach (ILanguage lang in _localizationService.GetAllLanguages())
        {
            var langId = lang.Id;
            IDictionaryTranslation? translation = source.Translations?.FirstOrDefault(x => x.LanguageId == langId);

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

        target.Translations = translations;

        target.CreateDate = source.CreateDate;
        target.UpdateDate = source.UpdateDate;
    }
}
